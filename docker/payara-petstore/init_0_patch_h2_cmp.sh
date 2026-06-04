#!/bin/bash
set -eu

CMP_GENERATOR_JAR="${PAYARA_DIR}/glassfish/modules/cmp-generator-database.jar"
TMP_DIR="$(mktemp -d)"

cleanup() {
  rm -rf "${TMP_DIR}"
}
trap cleanup EXIT

if jar tf "${CMP_GENERATOR_JAR}" | grep -q 'com/sun/jdo/spi/persistence/generator/database/H2.properties'; then
  echo "[PetStore] H2 CMP generator mapping already present"
  exit 0
fi

echo "[PetStore] Adding H2 CMP generator mapping based on SQL92.properties"
cd "${TMP_DIR}"
jar xf "${CMP_GENERATOR_JAR}" com/sun/jdo/spi/persistence/generator/database/SQL92.properties
cp \
  com/sun/jdo/spi/persistence/generator/database/SQL92.properties \
  com/sun/jdo/spi/persistence/generator/database/H2.properties
jar uf "${CMP_GENERATOR_JAR}" com/sun/jdo/spi/persistence/generator/database/H2.properties
