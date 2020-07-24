# Human Data Verification Protocol (HDVP)

The *Human Data Verification Protocol* is a tamper-proof(ish) way of transmitting arbitrary data over an insecure network. To verify that the data was not modified by a man-in-the-middle attacker, a human user must enter a short code on the receiver side. This code (called **verification code**) is basically a hash of transmitted data - but with some extensions (see below).

## Use Case

The primary use case for this protocol is to transfer the public key of a self-signed (HTTPS) certificate (or root CA) onto a freshly installed machine (computer). This certificate would then be used to secure any other traffic afterwards.

Because of this, the traffic for downloading the certificate can't be encrypted (e.g. regular HTTP rather than HTTPS). However, we would like to have (a certain degree of) confidence that the downloaded certificate hasn't been modified by a man-in-the-middle attacker. This is what HDVP does.

You *could* upload the certificate to a server (that runs HTTPS) and then download it on the new machine - but this requires an Internet connection (may or may not be a problem) and setting up a server and uploading a file securely (in an *automatic* way) is a lot of work. HDVP tries to solve this problem in a simple (yet still secure) way.

Also, HDVP is designed primarily for home or small business networks. Large companies have other (better = more effort) ways to deal with this problem (like pre-provisioned server images or physically secured networks).

## Design (Constraints)

The basic design of HDVP consists of:

* a server (e.g. a HTTP server) that hosts the binary data (so that it can be downloaded by the client); it also "displays" the **verification code** to the user.
* a client application: the user calls it (on the machine where to download the binary data to; e.g. the freshly installed server) with the ip address/hostname of the HDVP server and the verification code provided by the server. The application then downloads the binary data and verifies it against the verification code.

*Note:* The HDVP implementation actually just provides the APIs for creating and verifying the verification code. There is no network code in HDVP. It must be provided by the user of HDVP.

Next, the primary goal of HDVP is **integrity - not secrecy**. This means it is assumed that the transferred data is **not secret**. HDVP *only* verifies that the transferred data has **not been modified** - it does *not* keep the transferred data secret.

Based on the use case mentioned above, the **primary constraint** for the design of HDVP is that you can't use copy-paste - but have to **type everything**. Because of this, requiring the user to manually enter 60+ characters long hash is out of the question.

The main idea behind HDVP is to simply shorten the hash (of the binary data) to a "typeable" length. Since this weakens the hash (i.e. it becomes "easier" for an attacker to fabricate data that matches the shortened hash), HDVP implements various countermeasures to combat this.

In the end, an HDVP verification code may still be weaker than a regular hash. However, for the main use case there is a very limited time window (only minutes) where an attacker could launch an attack. This offsets this weakness.

### The Verification Code

The verification code is what the user enters on the client side. Thus, it should be "easily" typeable.

The length of the verification code can be configured (on the server side). For ease of use, a **length of 12 characters** is recommended. For security reasons, the **minimum length is 9** characters (8 for the hash and 1 for the length of the verification code). Due to the used encoding (see below), the **maximum length is 40** characters.

Under the hood, the verification code is series of bytes. These bytes need to be converted into a textual representation that a user can read and write. Because of its ease of use, the verification code is encoded with [z-base-32](http://philzimmermann.com/docs/human-oriented-base-32-encoding.txt) (a variation of [Base32](https://en.wikipedia.org/wiki/Base32)). This results in the following 32 characters being used in the verification code:

    ybndrfg8ejkmcpqxot1uwisza345h769

These are the English letters (a - z) and digits (0 - 9) without the letters `l` and `v`, and the digits `0` and `2`. Each character encodes 5 bits (2^5 = 32, hence Base32). The position in the string above defines which value is encoded with which character (e.g. 0 = y, 1 = b, 2 = n, ...)

Verification codes created by HDVP implementations should always be **lower-case** (for ease of use) but verification of the code should be case-insensitive (i.e. if a user wants to enter the code with upper-case characters the code would still be valid).

#### Why no shared secret?

An alternative way to implement the verification code would be to interpret it as shared secret (i.e. a password for encryption).

This way was not chosen because of the following difficulties:

* The HDVP server provides both the verification code as well as the binary data to transfer. If the verification code was secret, special measures would need to be taken to make sure that an attacker couldn't access the verification code.
* Encryption alone doesn't guarantee the integrity of the data. Thus, a hash would need to be incorporated into the binary data. And then we would need to make sure that we could detect changes by an attacker - and that an attack is still sufficiently difficult. The author finds that understanding the security properties of an encryption algorithm is a lot harder than understanding the security properties of a hash algorithm. Because of this (and because secrecy was never the goal for HDVP), the author was not comfortable with using encryption (over hashing).

### Definition: Attacking HDVP

The security design of HDVP assumes that an attacker can manipulate *any* communication between the HDVP server and the HDVP client. It also assumes that all information required to calculate a verification code is known to the attacker.

*Note:* It is assumed that the attacker can *only* manipulate the communication between the HDVP server and the HDVP client; i.e. that they can *neither* manipulate the communication between server and user nor between client and user.

A successful attack against HDVP is called **breaking the verification code**. In this case, the attacker successfully modifies the binary data in such a way that it generates the same verification code as the original data (this is called a [Preimage attack](https://en.wikipedia.org/wiki/Preimage_attack)). Additionally, the attack is considered successful *only* if the attacker managed to find *meaningful* binary data that generate the same verification code as the original data. Finding just some random *meaningless* data that matches the verification code is *not* considered a successful attack (because the attacker would not gain anything from this attack).

### Verification Code Design Step 1: Selecting the Hash Algorithm

The first (logical) step to implement the HDVP verification code is calculate the hash of the provided binary data.

The chosen cryptographic hash algorithm must meet the following conditions:

* It must still be considered secure against preimage attacks.
* It must be supported by the language in which HDVP is implemented and the implementation should ideally have been audited.
* It must be computational expensive (i.e. a "slow hash"); ideally calculating a hash should take about 0.5 second on a "regular" computer.

The last condition means that breaking the verification code through a brute-force attack takes considerably more time to execute than with a hash algorithm that's computational inexpensive. This is especially important in combination with the limited lifetime of the verification code (discussed in the step 3 below).

As a result the hash algorithm for HDVP is: **[Argon2id](https://en.wikipedia.org/wiki/Argon2)**.

[This post](https://security.stackexchange.com/a/216381/106930) was used as basis for the decision.

### Verification Code Design Step 2: Shorten the Hash

The hash needs to be shorted so that the user can easily type it.

To do this, HDVP first converts the whole hash into the z-base-32 encoding and then takes the first x characters (`desiredLength - 1`):

```c#
ConvertToZBase32(hash).Substring(0, desiredLength - 1)
```

Note that we reduce the length by 1. We use the first character to encode the length of the verification code. Otherwise, the HDVP client would need to *guess* the verification code length from what the user types. And this would leave the user vulnerable to typing a verification code that's too short (possibly only 1 character).

Since the minimum length for the verification code is specified with 9 characters (8 characters to the hash and 1 for the length), the first character encodes `desiredLength - 9`. Also, since we use a Base32 encoding, the maximum length for a verification code is 40 (`9 + (32 - 1)`).

```c#
ConvertToZBase32(desiredLength - 9)
```

### Verification Code Design Step 3: Salting the Hash

Based on the main use case mentioned above, let's assume the binary data is indeed (the public key of) a certificate. In this case it's very likely that the binary data won't change for a long time (i.e. the certificate is *not* recreated every couple of minutes).

This is problematic because in this case an attacker has a long time to try to break the verification code. The attacker would only then start their attack once they have broken the verification code.

To counteract this, HDVP "[salts](https://en.wikipedia.org/wiki/Salt_(cryptography))" the hash with some random binary data (the salt). The salt is changed every 5 minutes. This way, **the verification code changes every 5 minutes** (its "lifetime"). And since the salt is randomly chosen, an attacker can no longer predict the (next) verification code. This way, the time window for breaking a verification code is effectively limited to 5 minutes.

*Note:* The salt is not secret. It is transferred to the client over the same insecure network connection. The ability of an attacker to modify to salt does *not* increase the time frame they have to break the verification code as the next verification code can still not be guessed by the attacker.

It is the author's (unverified) impression that the ability for an attacker to arbitrarily modify the salt make it easier for them to break the verification code (within its lifetime). To make it harder for the attacker, the **salt is set to a fixed length of 32 bytes**. This way an attacker can't vary the length of the salt to get the desired verification code.

## Attack Estimation

Let's assume the attacker manipulates the binary data to something specific and then tries to break the verification code just by modifying the salt.

The salt is 32 bytes long. That's 256 bit or 2^256 different values.

Furthermore, one need to know that a (good) cryptographic hash algorithm can *only* be broken through a brute-force attack (i.e. just trying values).

The worst case for the attacker is try all these values and only the last value is the one that breaks the verification code. However, in "practice" the whole process is a process of chance (or luck). The more values the attacker tries, the higher the chance that the next value will break the verification code.

In cryptography there's something called a [birthday attack](https://en.wikipedia.org/wiki/Birthday_attack). It basically states that for a given `n` bits a collision can be found by trying `2^(n/2)` values. Assuming an 9 characters verification, that's `(9 - 1) * 5 = 40` bit. For 40 bit this would be `2^20` which is 1,048,576.

Given the 5 minute time frame mention above, the attacker would need to be able to calculate 3,495 hashes per second (but then could only run the attack if the user verified the binary data in the last second of the 5 minute time frame). This why it is important that HDVP uses a slow hash (with an estimation of 2 hashes per second).

For comparison, a 12 character verification code (55 bit) would require the attacker to calculate 632,708 hashes per second.
