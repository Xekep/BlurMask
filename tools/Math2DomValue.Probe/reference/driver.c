/* Test executable only. Netlib dtoa is not linked into the application. */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <fenv.h>
#define IEEE_8087
#define NO_BF96
#define strtod bm_reference_strtod
#include "dtoa.c"
#undef strtod

int main(int argc, char **argv)
{
    FILE *input, *output;
    char line[128], digits[64], slow[64], *end, *slow_end;
    unsigned long long bits;
    unsigned long count = 0;
    int precision, dp, sign, slow_dp, slow_sign;
    double value;
    if (argc != 3 || fegetround() != FE_TONEAREST) return 2;
    input = fopen(argv[1], "rb"); output = fopen(argv[2], "wb");
    if (!input || !output) return 3;
    while (fgets(line, sizeof(line), input)) {
        if (sscanf(line, "%llx %d", &bits, &precision) != 2 || (precision != 9 && precision != 17)) return 4;
        memcpy(&value, &bits, sizeof(value));
        if (!dtoa_r(value, 2, precision, &dp, &sign, &end, digits, sizeof(digits))) return 5;
        if (!dtoa_r(value, 6, precision, &slow_dp, &slow_sign, &slow_end, slow, sizeof(slow))) return 6;
        /* Mode6 disables the floating quick path. Compare full digit tuples. */
        if (dp != slow_dp || sign != slow_sign || strcmp(digits, slow)) return 7;
        if (fprintf(output, "%016llx\t%d\t%s\t%d\t%d\n", bits, precision, digits, dp, sign) < 0) return 8;
        count++;
    }
    if (ferror(input)) return 9;
    fclose(input); if (fclose(output)) return 10;
    printf("PASS %lu Netlib mode2/mode6 tuple comparisons (FE_TONEAREST, NO_BF96).\n", count);
    return 0;
}
