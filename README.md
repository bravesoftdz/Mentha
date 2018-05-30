# Mentha
An account balance aggregator for a few Canadian banks

Supported Banks
===============
Currently CIBC, Tangerine and TD are supported.  Simplii was supported up until the other day when they got hacked -- apparently they made changes to their API as I'm now getting a 403 when logging in, and I haven't had a chance to investigate yet.

Bank-specific Notes
===================

CIBC: Uses undocumented API, no user interaction required.

Simplii: Currently broken.  Previously used undocumented API, no user interaction required.

Tangerine: Uses undocumented API, first few logins will prompt for answer to additional security question (the answers are saved in Mentha's encrypted data file, so you should only be asked each question once and then on subsequent logins the stored answer will be used without prompting you)

TD: Uses "screen scraping" which means Mentha will pop up a web browser window, automatically initiate a login, "scrape" the information from the account summary page, and then perform a logout.  Please don't click anything while this takes place or the "injection" of your card numebr and password into the login form may not work

Security
========

When you save your first profile (ie card number and password) Mentha will prompt you to create a Master Password, which will be used to encrypt the Mentha data file.  The next time you run Mentha you will be required to re-enter this same master password in order to decrypt/load the data file.  This means if your Mentha data file is stolen by another person, they will not be able to open it unless they know your password, or are successful in "brute forcing" it, and the best defense against that is to use a strong master password.

For the specifics of how the encryption works you can see this file: https://github.com/rickparrish/Mentha/blob/master/Mentha/Code/Encryption.cs, or I'll try to explain briefly here:

- 34906 rounds of PBKDF2 are performed, using your master password and 16 bytes of "cryptographically strong random values" as salt, in order to generate a 16 byte pseudo-random key
- A random initialization vector (IV) is generated
- The 16 byte pseudo-random key, along with the random initialization vector, are used to encrypt the data using AES-128
- The iteration count, 16 bytes of salt, random initialization vector, and the encrypted data are then saved to disk

It's important to note two things:

1) Encryption, and security in general, is very easy to fuck up if you're not careful about what you're doing, especially if you don't specialize in this area (which I don't).  I believe the encryption routine to be free of any major flaws, but it's been put together after reading/referencing various things online, and there are A LOT of bad code samples out there (that I've hopefully avoided), but since the function has not been reviewed by anyone else there's no guarantees that it's not full of holes.

2) Your data is only encrypted on disk, and so it is only safe in cases where you share a computer with another person, or your laptop/desktop is lost/stolen, etc.  Card numbers and passwords will be stored in memory without protection after Mentha loads the data file, and so other rogue processes (ie malware) running on your computer could read this memory to steal your card numbers/passwords.  There are options for minimizing the amount of time a card number/password will be store in memory unprotected, but since at some point this information needs to be sent to your bank it is impossible to protect it 100% of the time, and that combined with the fact that malware could also just use keylogging to capture your master password and decrypt your Mentha data file means I'm not going to bother addressing this issue (unless someone has a good argument for bothering -- I'm not against doing it, just against doing it without a good reason)

Final Notes
===========

The various banks are free to change their undocumented APIs and/or HTML structure at any time, and so Mentha may break for any/all of them at any time.  Feel free to open an Issue on this repository if something breaks for you.
