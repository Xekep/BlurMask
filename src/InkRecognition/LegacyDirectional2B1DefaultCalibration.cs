using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Owned actual2B1 built-in defaults. Read-only native data; no SDK
///runtime dependency. Public copies cannot mutate the private catalog.</summary>
public static class LegacyDirectional2B1DefaultCalibration
{
    // Native0x1802b7610, SHA25648e3034ea277c9fa1dbbc6ffba6f841452ecd970803c7bc69bb83e6914d44a48.
    private static readonly short[] temporalMin=Decode("AATn/Qn//voI/pn+Cv9a/JT+mP2f/mP+bP5n/h342vag+nP6Evu3+u/7BfwQAwf7HPuQ/Pr7D/v6+nn8Zvqe+Mr5SPug+1n79fsI/DP7M/vb+nP8kPuc+YT5HPnj+bD5pfnj+sP71fvv+wb82vra+sb6/fx2+135Jvlp+WX57vlw+cL6sPsu/Nb71ftn+mf6u/pq/ZD74vjA+Dr51/jv+Y75r/q4+3P8yfu5+y36Lfq8+mH9pfu2+Jb4/vim+OD5l/mk+u77qPzE+6b7EfoR+tT6EP2R+6r4l/jO+J74w/mY+Zn6O/zO/L37kfv9+f359vq//Hf7lfih+Lv4lvip+af5mfp+/PH8p/t1+8X5xfkM+4X8d/tv+Hf4cvh5+I75uvmb+rX8G/2a+3T7t/m3+Rf7bPx5+3T4Z/hb+Iv4kfnO+aD65PxF/ZL7g/u5+bn5G/tj/IX7dfhw+FX4lvic+cD5pvoO/WT9ivuA+6b5pvkj+z78l/to+F/4W/ht+K/5hPmu+jv9hf16+2/7h/mH+Tz79vun+1n4PfhB+E74s/lu+bz6bP2h/XX7ZPto+Wj5Y/uk+7n7Wfgg+B34Pvi7+YL51Pqc/Zn9rPtY+5H5kfmw+zT72PuI+Gf4S/hp+MX5Ufn0+pH9Xf3m+2X74Png+Qr83voB/Lb41fin+Jf4yPld+R37GP0H/fn7f/u3+bIBW/zy+g/8+ftn+Yv60fpi+fr5fvs=");
    // Native0x1802b7aa0, SHA256fb4c595107c923d7280041710949079f5d867111057437cd59fc132f16c1833b.
    private static readonly short[] temporalMax=Decode("AAT3ABkCEv9VAeEADAFqAaYDgAFRCawICwnlCJIDLwXl/t/+dQIpAUP+/P0KBRb9bwPjBCwDBgf6BlIHlAbCAMH+9/zLAVoBBv7R/eH84fyLAyUFzAJA/3//jQC6/vf+/v6d/aIB5gH3/ez9Uf1R/QEDOgXOAqL/SACr/5L/mf5g/+f93gF/Agn+Pv70/fT9lAJFBRMDqgBFAef/vACP/kX/Gv4wAuwCK/5w/k/+T/6dAkQFNAMfAaIBYAA+AaX+Of85/nkCNwNL/oH+f/5//u4CKwUpAy4BdgG8AF0B1/4h/1P+vAJmA2v+j/6g/qD+PwMJBQYDWAFEAe4AcwEJ/wn/YP72An8Dnf6y/gj/CP95A/ME9wKuAZ4BmgG9ATj/9P5h/iMDjgO7/rb+I/8j/5ID6AQLA5oBwgG/AXkBNv/Q/lH+TQOqA8n+oP4g/yD/mwPkBC8DoAGwAbUBVAEh/+P+N/51A9UD3v6g/kb/Rv/AA9wEMgPfAdQBsgG4Afz+Uf8a/ocD+wMM/7L+iv+K/wkEwwQmAxECIgL5AREC8/6C//L9cgMfBDT/vf7R/9H/WwScBCUDBwJnAjQCMQLp/l3/uv1jA2QE/f6f/nD/cP/LBE8EKANVAawBrQHkAdT+rv91/dAD0AS7/lv+yv7K/iIF9AM6A74AuwD1AKUBzP6c/yr9AgVBBdP+Pv7Y/tsGDQWkA1sDIQgWBkUHawdf/4z+n/w=");
    // Native0x1802b7840, SHA256a8a6e49ffa0dfbe044d20c1cda2ed396750e5bd1c06901980b8b321f72622c7e.
    private static readonly short[] spatialMin=Decode("AATn/Qn//voI/pn+Cv9a/JT+mP2f/mP+bP5n/h342vag+nP6i/nI+Wj6c/rG+Cb6Hfos+p78kvit+Iz4efih+Pv5RPuk+kr6NPuk+pX45fl5+kv6X/xU+GP4N/iE+L74IPkn+xX7YPpK+7n6ufir+ZD6V/qy/Dr4l/ha+I34SvgB+Q370fph+uH6X/qm+L/4Pfp7+vr8yvmU+Df5VPjd9zj62Pr6+Sn6+Prs+rz4YPop+mn68vxK+TL48fjW+LX4Avkz+z/6M/r0+g77kflD+kP6Wvq1/E35+Pje+CD5J/jy9Tr7Wfo2+vH6/frX+er5N/pL+oj8z/hu+c34VvkX+O32Nfs8+sf52Ppx+iD6A/nk+eD5Jf17+MP52/hs+Uf5CvpJ+1T61/os+wv7wPi7+Uz68frD/GT5NvgU+bD4Cvik9/r6WPpR+u/6C/vV+QX6Rfpe+qX8afnu+ET5/viU9871Gvsm+g761Prk+j/6x/kQ+hv69/zn+HH5O/lI+fT3FPc0+yr6xPnR+mn6n/rt+Oj51PlB/Xf44/nU+IX5h/mF+VP7jvvG+5L7Tfvv+Kv44/pT++j8w/g2+qj41flZ+KD5wPpe+7X6g/vB+2H6/vjU+iL60Pzr+H/4KfmK+MH4/Pgk+xH7tPpm+777m/r9+Kb6HPqO/Lb4+vg5+Yb4R/lw+Un7Y/pU+uj6y/rB+gX5DPoF+pH8QflI+a74wfhD+s36bfs=");
    // Native0x1802b7cd0, SHA2560f6882f5871f4ef4d7e954b97af3c2e6d2dd7fdce5ad905b2b01a2f20eab0232.
    private static readonly short[] spatialMax=Decode("AAT3ABkCEv9VAeEADAFqAaYDgAFRCawICwnlCJIDLwXl/t/+zgQ1BGUB1P82BVr+pQE7A1sCCwLjBQkFzQLy/zz+8vzGBZAD0QAUAWcCyP56A4oCXAJsAlUC9QR9Adv/W/8v/dMFuwP9ACMC9gE4/6oDygLLAqEDZgF6BYYBcQB9/139ZASFBMwBQgM6A5ECJQKlA/cCBQiiAbsGGgPbAPH9l/2eAxIF2QANAJMB/v3cAD0EWALs/wUEywAgAez/gv8Z/ZsE+gPbABMBVf8l/i4CGQOBAtn/pADPADsAwwAlBBj9fATCA88ANwLb/rj+OgIRA0QC7gCR/wIByP/cAIoCIP0cAsIDJwAmA2P+0QD9AKkDQQJ0A//+SwGv/yz/MP71/CIEnwWCAXYBUQEX//0AqQQ0Arb/agWJAK8B4AB5AX39wwQOBBsBWwLb/o/+OQInA1QCnf/LAO7/pgClAWUES/0XBIMDpAAWAy7++/4ZAvQCoAK9AI3/CQD0/xkBZgIc/SkC2AMlAJUDof0eAc8AnANiAqwDwf4vAXb/1P7c/uD8XQbwBIUCYwMuAWsDgwLIA8ADbQHHBykCEAdkAKn+0/0sBpECyADUA/v93wCMBCwCtwKiAOABBwD1BOD/jP86/dMFOgKJAGYEp/3qAFkE/QGFAlsBiwDw/7ADJf/3/vb8CASEAn4AzQRy/aIE1wJVAm0CywX5/5AC/gPm/Uj9tfw=");
    public static short[] CopyMinimum(bool spatial)=>(short[])(spatial?spatialMin:temporalMin).Clone();
    public static short[] CopyMaximum(bool spatial)=>(short[])(spatial?spatialMax:temporalMax).Clone();
    internal static ReadOnlySpan<short> Minimum(bool spatial)=>spatial?spatialMin:temporalMin;
    internal static ReadOnlySpan<short> Maximum(bool spatial)=>spatial?spatialMax:temporalMax;
    private static short[] Decode(string encoded)
    {
        byte[] raw=Convert.FromBase64String(encoded);if(raw.Length!=548)throw new InvalidDataException("Actual274-short native calibration required.");
        var result=new short[274];for(int i=0;i<result.Length;i++)result[i]=BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(i*2));return result;
    }
}
