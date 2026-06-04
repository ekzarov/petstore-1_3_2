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

Admin console:

```text
http://localhost:8080/admin
```

## Current Scope

The image deploys the prebuilt legacy EARs by default:

```text
petstore.ear
opc.ear
supplier.ear
petstoreadmin.ear
```

The image creates separate H2 JDBC resources for the legacy application
databases:

```text
jdbc/petstore/PetStoreDB
jdbc/opc/OPCDB
jdbc/supplier/SupplierDB
```

The image also creates the JMS queues, topic, connection factories, and JavaMail
session needed by the order-processing, supplier, and admin flows.

## Why The Patch Exists

Payara 5.2021.1 uses H2 as its default database, but its legacy EJB CMP code
generator does not ship an `H2.properties` mapping resource. The init script
adds `H2.properties` based on Payara's bundled `SQL92.properties`, matching the
manual smoke test that successfully deployed `petstore.ear`.

The prebuilt PetStore EARs contain old `sun-j2ee-ri.xml` descriptors. Payara 5
does not use their `<cmpresource>` entries, so the Docker build injects
`META-INF/glassfish-ejb-jar.xml` into CMP EJB modules to point each application
area at its own JDBC resource.

The same injection step adds Payara runtime mappings for legacy JMS MDB
destinations and AsyncSender resources that were originally described only in
`sun-j2ee-ri.xml`.
