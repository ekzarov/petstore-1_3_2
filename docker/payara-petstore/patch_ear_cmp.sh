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
  jdbc/petstore/PetStoreDB) descriptor_dir="/opt/payara/petstore-descriptors/petstore" ;;
  jdbc/opc/OPCDB) descriptor_dir="/opt/payara/petstore-descriptors/opc" ;;
  jdbc/supplier/SupplierDB) descriptor_dir="/opt/payara/petstore-descriptors/supplier" ;;
  admin) descriptor_dir="/opt/payara/petstore-descriptors/admin" ;;
  *) echo "No descriptor configured for ${jndi_name}" >&2; exit 1 ;;
esac

cd "${work_dir}"
jar xf "${ear_path}"

for module in "$@"; do
  if [ ! -f "${module}" ]; then
    echo "[PetStore] Skipping missing module ${module} in ${ear_path}"
    continue
  fi

  descriptor="${descriptor_dir}/${module}.xml"
  if [ ! -f "${descriptor}" ]; then
    descriptor="${descriptor_dir}/glassfish-ejb-jar.xml"
  fi

  rm -rf descriptor
  case "${module}" in
    *.war)
      mkdir -p descriptor/WEB-INF
      cp "${descriptor}" descriptor/WEB-INF/glassfish-web.xml
      echo "[PetStore] Adding Payara web runtime descriptor ${descriptor} to ${module}"
      (cd descriptor && jar uf "../${module}" WEB-INF/glassfish-web.xml)
      ;;
    *)
      mkdir -p descriptor/META-INF
      cp "${descriptor}" descriptor/META-INF/glassfish-ejb-jar.xml
      echo "[PetStore] Adding Payara EJB runtime descriptor ${descriptor} to ${module}"
      (cd descriptor && jar uf "../${module}" META-INF/glassfish-ejb-jar.xml)
      ;;
  esac
done

rm -rf descriptor
rm -f "${ear_path}"
jar cfm "${ear_path}" META-INF/MANIFEST.MF .
