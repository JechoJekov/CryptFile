# CryptFile
A command-line tool to encrypt/decrypt files. Files are encrypted with AES-256. The encryption key is derived from the password by using BCrypt (work factor 10) and PBKDF2 is used to derive a 256 bit key from the 192 output returned by the BCrypt algorithm.
