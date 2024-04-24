#!/bin/bash

sleep 60

# /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -i /usr/src/scripts/key-store.sql

# sleep 10

/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -i /usr/src/scripts/database.sql