# The HDVP Specification

This file contains the HDVP specification.

For reasoning behind the various parts, see the [design notes](README.md).

You can find the reference implementation (in C#) in the `impl` directory.

**Specification Version:** 1.0

## General Overview

HDVP consists of two "sides":

* The sender side provides some data and **generates** a verification code.
* The receiver side receives the data (via network) and **verifies** it against the verification code manually entered by the user.

Additionally, there are two important constants:

* `min_code_length = 9`: The minimum verification code length.
* `max_code_length = 40`: The maximum verification code length. Calculated from: `min_code_length` + characters in z-base-32 (`32`) - `1`

### Generate a Verification Code

To generate an HDVP verification code for some data, you need:

* `input_data`: the binary data you want to transmit (and verify on the other side)
* `code_length`: the desired length (number of characters) of the generated verification code (must be `min_code_length <= code_length <= max_code_length`); default is `12`
* `code_ttl`: the time-to-live of each verification code; defaults to 1 minute

You also need a **32 byte salt**. This salt needs to:

* be cryptographically secure (i.e. generated using a cryptographically secure random number generator)
* change every `code_ttl`

To generate a verification code, the following steps must be executed:

1. Calculate the **SHA-512 hash** from `input_data`.
1. Calculate the **required number of bytes for the Argon2 hash** based on `code_length - 1`.
1. Calculate the **Argon2id hash** (byte count from the previous step) from the hash from step 1 and the salt.
1. Encode the Argon2 hash with **z-base-32** (no padding) and truncate it to `code_length - 1`.
1. **Encode `code_length - min_code_length`** into a single z-base-32 character and prepend it to the verification code from the previous step.

### Verify a Verification Code

To verify some binary data against a verification code, you need:

* `received_data`: the binary data to verify (transmitted via network)
* `received_salt`: the salt used to create the verification code (transmitted via network)
* `verification_code`: the verification code (entered manually by the user)

Then, the following steps must be executed:

1. Optionally verify that `verification_code`'s **length** is a greater than or equal to `min_code_length` and less than or equal to `max_code_length`.
1. **Decode** the first character of the `verification_code` via z-base-32 and verify that the verification code is `decoded_length + min_code_length` characters long.
1. Optionally verify that `verification_code` contains **only z-base-32 characters**.
1. **Generate a verification code** from `received_data`, `received_salt`, and `decoded_length + min_code_length`
1. **Compare** the generated verification code with `verification_code`.

If the codes match, the `received_data` has not been modified in transit.

## Argon2

HDVP uses **[Argon2](https://en.wikipedia.org/wiki/Argon2)** as slow hash.

Argon2 comes in 3 variants:

* Argon2d
* Argon2i
* Argon2id

HDVP uses **Argon2id**.

HDVP uses the following parameters for Argon2:

* `parallelism` (number of threads): `8`
* `memorySizeKB`: `150,000` (~ 150 MB)
* `iterations`: `2`

With these parameters, at the time of writing (2020), a regular desktop CPU can calculate about 4 hashes per second. A Raspberry Pi 4 calculates 0.3 hashes per second.

### Calculating the Number of Required Bytes

When creating an Argon2 hash, you need to specify how many bytes you want to have generated.

You may use the following algorithm (implemented in C#) to calculate the number of required hash bytes from the desired code length.

```c#
public int GetRequiredByteCount(int expectedStringLength)
{
    int groupCount;

    if (expectedStringLength % SYMBOLS_PER_GROUP == 0)
    {
        groupCount = expectedStringLength / SYMBOLS_PER_GROUP;
    }
    else
    {
        groupCount = (int)Math.Ceiling((double)expectedStringLength / SYMBOLS_PER_GROUP);
    }

    return groupCount * BYTES_PER_GROUP;
}
```

With:

* `expectedStringLength` is `code_length - 1`
* `SYMBOLS_PER_GROUP` is `8`
* `BYTES_PER_GROUP` is `5`

## z-base-32

HDVP uses [z-base-32](http://philzimmermann.com/docs/human-oriented-base-32-encoding.txt) as encoding for the verification code.

The following characters encode the values 0 to 31 in z-base-32:

    ybndrfg8ejkmcpqxot1uwisza345h769

These are the English letters (a - z) and digits (0 - 9) without the letters `l` and `v`, and the digits `0` and `2`.

The position in the string above defines which value is encoded with which character (e.g. 0 = y, 1 = b, 2 = n, ...)
