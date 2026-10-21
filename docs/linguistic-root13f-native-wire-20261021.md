# Native linguistic13F wire grammar

This static checkpoint proves only the native13F/102 wire layout and fixed parent133/100 prefix consumed by the separate opaque library reader. It does not activate resources, accept their licensing-related fields, select linguistic sources or recognize text. The vendor DLL is read as PE bytes, never loaded or executed.

The registered13F descriptor2E5310 has parent133 and binds its readerV10 to1D1240. Version102 calls registered133.V10=1D2680 with version100. That parent reads4 bytes to receiver+10, a fixed56-byte name buffer, then a signed32-bit sentinel. A sentinel-1 consumes a separate4-byte raw4C field; otherwise the sentinel is raw4C. It then always consumes4-byte raw50. Thus serialized parent length is72 or68 bytes, with no variable-length name encoding. These fields remain opaque in the wire reader. Native licensing/acceptance paths in the rest of133 are not implemented or imitated.

Only after successful parent reading,1D1240 reads a uint32 child count and calls the generic object reader for each child in order. Versions>=101 align the stream to4 bytes between children, never after the final child. The actual padding is not necessarily zero. The root-file-offset0 scope avoids inventing alignment relative to an unknown nested stream origin. Unknown child kinds remain opaque declared envelopes; this checkpoint does not prove their internal readers.

1D22E0 receives the exact56-byte buffer. With sentinel-1, its initial instructions use little-endian XOR key77191214, giving repeating bytes14/12/19/77; otherwise the buffer is copied. Name decoding is a static inventory observation and is deliberately absent from the separate pure layout reader. The native decoder's wider string/invalid-byte behavior is not asserted by a UTF8 display here.

The complete Ghidra body address sets of1D1240,1D2680 and1D22E0 were exported read-only with no new analysis. Their3 exact PE regions,35 relevant instruction pins and8 registration/table pins are checked independently. No first-unwind prefix is called a full function. Native allocation/read failures keep prefixes and release objects as shown in the full reader; the library's all-or-nothing owned wire snapshot is a separate documented contract, not an allocator ABI port.

For the pinned RU resource, the root name is ru_RU-lk-text.lite; the actual prefix is72 bytes, count22 is at88 and children begin92. There are19 children13A/202,2 children13B/300 and1 child13D/10A. Actual13B envelopes are606964..4565174 and4575280..6051538;13D is4575188..4575278. Three interchild padding regions containFFFF. The pinned English root has21 children. All root/child flags are0 in these exact resources. Sequential extent walking consumes the complete files; no tag-signature scanning is used.

The presence of these children does not alone authorize an automatic recognition join. The fullroot resource-collection observers and configured/source-selection factory are a separate in-progress static checkpoint. Current selected-only recognizers must not be silently widened by this reader.

Audit: python tools/handwriting/audit_linguistic_root13f_wire.py --sdk SDK-DIRECTORY --models MODELS-DIRECTORY --output REPORT. Native exports and full inventory stay under E:; the frozen manifest records hashes. No targets or recognition quality result are used.
