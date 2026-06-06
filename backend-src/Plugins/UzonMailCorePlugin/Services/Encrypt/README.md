# Developer Documentation

This folder is responsible for secret management.

## User Secret Encryption Flow

1. Key and IV are generated from the user's password.
2. Encrypt the user's key and IV with the system key and IV (to prevent database leakage).
3. Store the result in the database.

## Password Change

When the user changes their password, the service must decrypt and re-encrypt all stored passwords.