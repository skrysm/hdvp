# Human Data Verification Protocol (HDVP)

[![NuGet](https://img.shields.io/nuget/v/HDVP.svg)](https://www.nuget.org/packages/HDVP/)

The *Human Data Verification Protocol* provides a way to securely transmit arbitrary data over an insecure network.

"Securely" in this case means that the data is not modified in transit (integrity). However, the data is *not* encrypted (confidentiality).

To verify that the data was not modified by a man-in-the-middle attacker, a human user must enter a short code on the receiver side. This code (called an **HDVP verification code**) is basically a hash of transmitted data - but with some modifications to make it easier to use (see design notes).

## Use Case

The primary use case for this protocol is to transfer the public key of a self-signed (HTTPS) certificate (or root CA) onto a freshly installed machine (computer). This certificate would then be used to secure any other traffic afterwards.

In this scenario, the traffic for downloading the certificate can't be encrypted (e.g. regular HTTP rather than HTTPS). However, we would like to have (a certain degree of) confidence that the downloaded certificate hasn't been modified by a man-in-the-middle attacker. This is what HDVP does.

You *could* upload the certificate to a server (that runs HTTPS with a CA signed certificate) and then download it on the new machine - but this requires an Internet connection (may or may not be a problem) and setting up a server and uploading a file securely (in an *automatic* way) is a lot of work. HDVP tries to solve this problem in a simple (yet still secure) way.

Also, HDVP is designed primarily for home or small business networks. Large companies have other (better = more effort) ways to deal with this problem (like pre-provisioned server images or physically secured networks).

## The Basic Flow

The basic flow of HDVP is as follows:

1. You provide HDVP with the binary data to transmit.
1. HDVP provides you with a 32 byte salt and a (human-readable) verification code.
1. You download the binary data and the salt to the target machine.
1. On the target machine, you provide HDVP with the downloaded data, salt and enter the verification code.
1. HDVP tells you whether the verification code matches the downloaded data.

Note: HDVP itself does *not* provide any means to transmit the binary data or the salt to the target machine. HDVP just provides the verification protocol.

To increase security, the verification code is only valid for a certain time (by default, 1 minute).

## The Verification Code

The verification code is human-readable and human-typeable.

Its length is configurable (9 - 40 characters).

A verification looks like this:

    dry9odphhaa3

## Using the C# Implementation

The directory `src` in this repository contains the HDVP reference implementation (in C#).

On the server side (that provides binary data to download), you first need to read the binary data into an instance of `HdvpVerifiableData`. For this, use either `HdvpVerifiableData.ReadFromMemory()` or `HdvpVerifiableData.ReadFromStream()`.

```c#
byte[] myDataAsByteArray = ...
var myVerifiableData = HdvpVerifiableData.ReadFromMemory(myDataAsByteArray);
```

Next, with this instance you create an instance of `HdvpVerificationCodeProvider`:

```c#
var codeProvider = new HdvpVerificationCodeProvider(myVerifiableData);
```

To get the currently valid verification code (remember that validation codes are only valid for a certain amount of time), use the `GetVerificationCode()` method:

```c#
HdvpVerificationCode currentVerificationCode = codeProvider.GetVerificationCode();
```

You can check for how long the current verification code is valid via `codeProvider.VerificationCodeValidUntilUtc`.

The `HdvpVerificationCode` class provides you with two important properties:

* `Code`: the verification code as a string
* `Salt`: the salt of the verification code

You can now display the `Code` to the user and provide the `Salt` as download for the client.

On the client side, you then:

1. download the binary data from the server
1. download the salt from the server
1. let the user enter the verification code

With the entered code, you first want to checked whether its format is correct:

```c#
var verificationCodeCheckResult = HdvpVerificationCode.CheckFormat(verificationCodeAsString);
if (verificationCodeCheckResult != HdvpFormatValidationResults.Valid)
{
    // Error reporting here
}
```

The last step is to verify the binary data against the verification code the user entered:

```c#
var data = HdvpVerifiableData.ReadFromMemory(...);
var hdvpVerificationCode = HdvpVerificationCode.Create(verificationCodeAsString, new HdvpSalt(salt));
if (!hdvpVerificationCode.IsMatch(verifiableData))
{
    // Error handling here
}
```

*Notes:*

* It's recommended to download the salt *before* asking the user to enter the verification. This is because the salt is changed every time the verification code changes and by downloading it before asking the user to enter the verification, it doesn't matter if the user is too slow to enter the code.
* It's also recommended (for the same reason) to re-download the code *every time* before asking the user to enter the verification code.

## Other Documentation

The specification of HDVP can be found in [SPEC.md](SPEC.md).

Background information on the design of HDVP can be found in [DESIGN-NOTES.md](DESIGN-NOTES.md).
