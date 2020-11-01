# Human Data Verification Protocol (HDVP)

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

## Next Steps

The directory `impl` in this repository contains the HDVP reference implementation (in C#).

The specification of HDVP can be found in [SPEC.md](SPEC.md).

Background information on the design of HDVP can be found in [DESIGN-NOTES.md](DESIGN-NOTES.md).
