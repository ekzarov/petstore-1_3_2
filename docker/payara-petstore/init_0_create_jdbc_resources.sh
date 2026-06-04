#!/bin/bash
set -eu

touch "${POSTBOOT_COMMANDS}"

append_once() {
  command="$1"
  marker="$2"

  if grep -q "${marker}" "${POSTBOOT_COMMANDS}"; then
    echo "[PetStore] JDBC command already present: ${marker}"
    return
  fi

  echo "${command}" >> "${POSTBOOT_COMMANDS}"
}

create_h2_pool() {
  pool_name="$1"
  db_name="$2"
  jndi_name="$3"

  append_once \
    "create-jdbc-connection-pool --datasourceclassname org.h2.jdbcx.JdbcDataSource --restype javax.sql.DataSource --property URL=jdbc\\:h2\\:\\\${com.sun.aas.instanceRoot}/lib/databases/${db_name}\\;AUTO_SERVER\\=TRUE ${pool_name}" \
    " ${pool_name}"

  append_once \
    "create-jdbc-resource --connectionpoolid ${pool_name} ${jndi_name}" \
    "${jndi_name}"
}

create_h2_pool "PetStorePool" "petstore" "jdbc/petstore/PetStoreDB"
create_h2_pool "OPCPool" "opc" "jdbc/opc/OPCDB"
create_h2_pool "SupplierPool" "supplier" "jdbc/supplier/SupplierDB"
