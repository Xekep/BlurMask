if(args is ["--index-only",var models,var output]) { Node0IndexChecks.Run(models,output); return; }
if(args.Length!=5||args[0]!="--actual-node0")throw new ArgumentException("--actual-node0 <models> <raw-input-json> <output-json> <134|100>");
Node0Capture.Run(args[1],args[2],args[3],args[4]);
