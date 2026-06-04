# PetStore Docker Runtime

This is a reproducible Docker wrapper for the prebuilt Java Pet Store EAR.
It does not rebuild or modify the legacy Java source.

## Start

```sh
docker compose up --build
```

Open:

```text
http://localhost:8080/petstore
```

Payara admin console:

```text
http://localhost:4848
user: admin
password: admin
```

## Current Scope

The image deploys only `petstore.ear`.

`opc.ear` and `supplier.ear` need separate datasource/CMP configuration before
they can be deployed together because their CMP entity tables currently collide
in Payara's default H2 database.

JMS resources also still need to be created for the complete order-processing
flow.

## Why The Patch Exists

Payara 5.2021.1 uses H2 as its default database, but its legacy EJB CMP code
generator does not ship an `H2.properties` mapping resource. The init script
adds `H2.properties` based on Payara's bundled `SQL92.properties`, matching the
manual smoke test that successfully deployed `petstore.ear`.
