#!/bin/bash
set -eu

ear_path="$1"
jndi_name="$2"
shift 2

work_dir="$(mktemp -d)"

cleanup() {
  rm -rf "${work_dir}"
}
trap cleanup EXIT

case "${jndi_name}" in
  jdbc/petstore/PetStoreDB) descriptor="/opt/payara/petstore-descriptors/petstore/glassfish-ejb-jar.xml" ;;
  jdbc/opc/OPCDB) descriptor="/opt/payara/petstore-descriptors/opc/glassfish-ejb-jar.xml" ;;
  jdbc/supplier/SupplierDB) descriptor="/opt/payara/petstore-descriptors/supplier/glassfish-ejb-jar.xml" ;;
  *) echo "No descriptor configured for ${jndi_name}" >&2; exit 1 ;;
esac

cd "${work_dir}"
jar xf "${ear_path}"

mkdir -p descriptor/META-INF
cp "${descriptor}" descriptor/META-INF/glassfish-ejb-jar.xml

for module in "$@"; do
  if [ ! -f "${module}" ]; then
    echo "[PetStore] Skipping missing module ${module} in ${ear_path}"
    continue
  fi

  echo "[PetStore] Adding CMP datasource ${jndi_name} to ${module}"
  (cd descriptor && jar uf "../${module}" META-INF/glassfish-ejb-jar.xml)
done

rm -rf descriptor
rm -f "${ear_path}"
jar cfm "${ear_path}" META-INF/MANIFEST.MF .
