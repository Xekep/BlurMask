// @category BlurMask.PortAnalysis
// Static export only. Never creates functions or modifies the analyzed program.
import ghidra.app.script.GhidraScript;
import ghidra.app.decompiler.*;
import ghidra.framework.Application;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.*;
import ghidra.program.model.symbol.*;
import com.google.gson.*;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.*;

public class ExportPortBatch extends GhidraScript {
    private String hash(Path path) throws Exception {
        byte[] bytes = MessageDigest.getInstance("SHA-256").digest(Files.readAllBytes(path));
        return HexFormat.of().formatHex(bytes);
    }

    private String clean(String text) {
        return text.replace("\t", " ").replace("\r", " ").replace("\n", " ");
    }

    private void write(Path path, String text) throws Exception {
        Files.writeString(path, text, StandardCharsets.UTF_8, StandardOpenOption.CREATE_NEW);
    }

    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length != 2) throw new IllegalArgumentException("REQUEST_JSON OUTPUT_DIRECTORY");
        Path out = Paths.get(args[1]);
        JsonObject request = JsonParser.parseString(Files.readString(Paths.get(args[0]))).getAsJsonObject();
        String expected = request.get("binarySha256").getAsString();
        if (!expected.equalsIgnoreCase(currentProgram.getExecutableSHA256()))
            throw new IllegalStateException("Project program does not match requested binary SHA256");
        Map<String, Object> report = new LinkedHashMap<>();
        report.put("schema", 1);
        report.put("program", currentProgram.getName());
        report.put("binarySha256", currentProgram.getExecutableSHA256().toLowerCase(Locale.ROOT));
        report.put("ghidraVersion", Application.getApplicationVersion());
        report.put("imageBase", currentProgram.getImageBase().toString());
        report.put("language", currentProgram.getLanguageID().toString());
        report.put("compilerSpec", currentProgram.getCompilerSpec().getCompilerSpecID().toString());
        report.put("vendorExecution", false);
        report.put("programMutation", false);
        report.put("status", "running");
        List<Map<String, Object>> rows = new ArrayList<>();
        report.put("functions", rows);
        Gson gson = new GsonBuilder().setPrettyPrinting().create();
        DecompInterface decompiler = new DecompInterface();
        DecompileOptions options = new DecompileOptions();
        options.grabFromProgram(currentProgram);
        decompiler.setOptions(options);
        try {
            if (!decompiler.openProgram(currentProgram)) throw new IllegalStateException("Decompiler open failed");
            boolean allPassed = true;
            for (JsonElement item : request.getAsJsonArray("addresses")) {
                monitor.checkCancelled();
                String value = item.getAsString();
                Address address = toAddr(value);
                Map<String, Object> row = new LinkedHashMap<>();
                row.put("address", value);
                rows.add(row);
                Function function = getFunctionAt(address);
                if (function == null || !function.getEntryPoint().equals(address)) {
                    row.put("status", "missing-function-entry");
                    Function containing = getFunctionContaining(address);
                    if (containing != null) {
                        row.put("containingEntry", containing.getEntryPoint().toString());
                        row.put("containingName", containing.getName());
                    }
                    allPassed = false;
                    continue;
                }
                row.put("name", function.getName());
                row.put("signature", function.getSignature().toString());
                row.put("bodyAddresses", function.getBody().getNumAddresses());
                DecompileResults result = decompiler.decompileFunction(function,
                    request.get("decompileSeconds").getAsInt(), monitor);
                if (!result.decompileCompleted() || result.getDecompiledFunction() == null) {
                    row.put("status", "decompile-failed");
                    row.put("error", result.getErrorMessage());
                    allPassed = false;
                    continue;
                }
                String stem = value.substring(2);
                Path c = out.resolve(stem + ".c");
                Path asm = out.resolve(stem + ".instructions.tsv");
                Path refs = out.resolve(stem + ".references.tsv");
                write(c, result.getDecompiledFunction().getC());
                StringBuilder instructions = new StringBuilder("address\tbytes\tinstruction\n");
                List<String> references = new ArrayList<>();
                ReferenceIterator inbound = currentProgram.getReferenceManager().getReferencesTo(address);
                while (inbound.hasNext()) {
                    Reference ref = inbound.next();
                    references.add("in\t" + ref.getFromAddress() + "\t" + ref.getToAddress() + "\t" + ref.getReferenceType());
                }
                InstructionIterator iterator = currentProgram.getListing().getInstructions(function.getBody(), true);
                int instructionCount = 0;
                while (iterator.hasNext()) {
                    monitor.checkCancelled();
                    Instruction instruction = iterator.next();
                    instructionCount++;
                    instructions.append(instruction.getAddress()).append('\t')
                        .append(HexFormat.of().formatHex(instruction.getBytes())).append('\t')
                        .append(clean(instruction.toString())).append('\n');
                    for (Reference ref : instruction.getReferencesFrom())
                        references.add("out\t" + ref.getFromAddress() + "\t" + ref.getToAddress() + "\t" + ref.getReferenceType());
                }
                Collections.sort(references);
                write(asm, instructions.toString());
                write(refs, "direction\tfrom\tto\ttype\n" + String.join("\n", references) + "\n");
                Map<String, String> artifacts = new LinkedHashMap<>();
                for (Path path : List.of(c, asm, refs)) artifacts.put(path.getFileName().toString(), hash(path));
                row.put("artifacts", artifacts);
                row.put("instructions", instructionCount);
                row.put("status", instructionCount > 0 ? "passed" : "no-instructions");
                if (instructionCount == 0) allPassed = false;
                println("Exported " + value + " " + function.getName());
            }
            report.put("status", allPassed ? "passed" : "failed");
        } catch (Exception error) {
            report.put("status", "failed");
            report.put("error", error.toString());
            throw error;
        } finally {
            decompiler.dispose();
            write(out.resolve("export-index.json"), gson.toJson(report) + "\n");
        }
    }
}
