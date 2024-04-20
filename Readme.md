# Concurrent key signing service

*Problem Statement*
Implement a record signing service using a message driven / microservice solution.

Given a database of 100,000 records and a collection of 100 private keys, create a process to concurrently sign batches of records, storing the signatures in the database until all records are signed.

*Rules*

* No double signing: Only a signature per record should be stored (sign each record individually in batches of X size)
* Any given key in the keyring must not be used concurrently
* A single key should be used for signing all records in a single batch
* Keys should be selected from least recently used to most recently
* Batch size should be configurable by the user (does not change during runtime)

*Guidelines*

* Use a runtime environment of your choosing (we predominantly use Golang and Typescript but language knowledge assessment is not the aim of this challenge)
* Use any orchestration or process coordination tools you see fit (message queues, lambdas, etc)
* Seed the records with any random data
* Use a public key crypto algorithm of your choosing


## High level overview

*Core Services*
* *Collection Service*: Get collection of data from the database.
* *Key Management Service*: Retrieve and lock the key from the database and unlock the key.
* *Signing Service*: Sign and store the signed document.
* *Signing Executor*: Execute the signing to the bulk data.

*Other tools*
* *Data Seeder*: Seed data to the databases.

