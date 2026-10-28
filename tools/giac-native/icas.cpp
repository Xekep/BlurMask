// SPDX-License-Identifier: GPL-3.0-or-later
// Minimal stdin/stdout Giac transport for NovaPen. No GUI or prompt.
#include "giac.h"
#include "tex.h"
#include "vecteur.h"
#include <iostream>
#include <string>
#include <cstring>

// The minimal GIAC_GGB build uses Giac's legacy TeX formatter. Preserve the
// evaluated values but transport lists and matrices in a single-line format.
// A negative integer's legacy \mathrm wrapper is presentation, not a symbol.
static std::string normalize_numeric_wrappers(std::string text) {
    const std::string prefix = "\\mathrm{";
    std::size_t offset = 0;
    while ((offset = text.find(prefix, offset)) != std::string::npos) {
        const auto end = text.find('}', offset + prefix.size());
        if (end == std::string::npos) break;
        const auto value = text.substr(offset + prefix.size(), end - offset - prefix.size());
        std::size_t index = (!value.empty() && value[0] == '-') ? 1 : 0;
        const auto first_digit = index;
        for (; index < value.size() && value[index] >= '0' && value[index] <= '9'; ++index) {}
        if (index == value.size() && index > first_digit) {
            text.replace(offset, end + 1 - offset, value);
            offset += value.size();
        } else offset = end + 1;
    }
    return text;
}

static std::string format_math(const giac::gen &value, giac::context *context, unsigned depth = 0) {
    if (depth > 64) throw std::runtime_error("Result nesting limit");
    if (value.type == giac::_FRAC) {
        const auto &numerator = value._FRACptr->num;
        const bool negative = (numerator.type == giac::_INT_ || numerator.type == giac::_ZINT) &&
            !numerator.print(context).empty() && numerator.print(context)[0] == '-';
        return std::string(negative ? "-" : "") + "\\frac{" +
            format_math(negative ? -numerator : numerator, context, depth + 1) + "}{" +
            format_math(value._FRACptr->den, context, depth + 1) + "}";
    }
    if (value.type != giac::_VECT) return normalize_numeric_wrappers(giac::gen2tex(value, context));
    const auto &items = *value._VECTptr;
    if (items.size() > 1024) throw std::runtime_error("Result size limit");
    if (value.subtype != giac::_LIST__VECT && !items.empty() && giac::ckmatrix(value)) {
        const auto columns = items.front()._VECTptr->size();
        std::string result = "\\left[\\begin{array}{" + std::string(columns, 'c') + "}";
        for (std::size_t row = 0; row < items.size(); ++row) {
            if (row) result += "\\\\";
            for (std::size_t column = 0; column < columns; ++column) {
                if (column) result += '&';
                result += format_math((*items[row]._VECTptr)[column], context, depth + 1);
            }
        }
        return result + "\\end{array}\\right]";
    }
    std::string result = "\\left[";
    for (std::size_t index = 0; index < items.size(); ++index) {
        if (index) result += ',';
        result += format_math(items[index], context, depth + 1);
    }
    return result + "\\right]";
}

int main(int argc, char **argv) {
    if (argc == 2 && std::strcmp(argv[1], "--version") == 0) {
        std::cout << "NovaPen Giac CLI (GeoGebra giac bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c)\n";
        return 0;
    }
    if (argc > 2 || (argc == 2 && std::strcmp(argv[1], "--sage") != 0)) {
        std::cerr << "Usage: icas [--sage]\n";
        return 2;
    }
    giac::context context;
    std::string line;
    while (std::getline(std::cin, line)) {
        if (line.size() > 65536) return 2;
        try {
            const giac::gen expression(line, &context);
            if (expression.is_symb_of_sommet(giac::at_latex)) {
                const giac::gen result = giac::eval(expression._SYMBptr->feuille, 1, &context);
                const auto text = format_math(result, &context);
                if (text.size() > 65536 || text.find_first_of("\r\n\t\"") != std::string::npos)
                    throw std::runtime_error("Result transport limit");
                // Literal TeX backslashes, following Giac's CLI protocol (not JSON).
                std::cout << '"' << text << '"' << '\n';
            } else {
                const giac::gen result = giac::eval(expression, 1, &context);
                std::cout << result.print(&context) << '\n';
            }
        } catch (const std::exception &error) {
            std::cerr << error.what() << '\n';
            return 1;
        }
    }
    return 0;
}
