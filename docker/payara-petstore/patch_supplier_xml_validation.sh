#!/bin/bash
set -eu

ear_path="$1"
module="supplier-ejb.jar"
work_dir="$(mktemp -d)"

cleanup() {
  rm -rf "${work_dir}"
}
trap cleanup EXIT

cd "${work_dir}"
jar xf "${ear_path}" "${module}"

if [ ! -f "${module}" ]; then
  echo "[PetStore] Missing ${module} in ${ear_path}" >&2
  exit 1
fi

mkdir module
(cd module && jar xf "../${module}" \
  META-INF/ejb-jar.xml \
  com/sun/j2ee/blueprints/supplier/rsrc/xsl/TPASupplierOrder.xsl)

perl -0pi -e 's|(<env-entry-name>param/xml/validation/SupplierOrder</env-entry-name>\s*<env-entry-type>java\.lang\.Boolean</env-entry-type>\s*<env-entry-value>)true(</env-entry-value>)|${1}false${2}|s' module/META-INF/ejb-jar.xml

xsl_path="module/com/sun/j2ee/blueprints/supplier/rsrc/xsl/TPASupplierOrder.xsl"
perl -0pi -e 's#\@categoryId#\@categoryId | \@tpali:categoryId#g; s#\@productId#\@productId | \@tpali:productId#g; s#\@itemId#\@itemId | \@tpali:itemId#g; s#\@lineNo#\@lineNo | \@tpali:lineNo#g; s#\@quantity#\@quantity | \@tpali:quantity#g; s#\@unitPrice#\@unitPrice | \@tpali:unitPrice#g' "${xsl_path}"

if ! grep -q '<?xml version="1.0"' "${xsl_path}"; then
  echo "[PetStore] Supplier XSL patch corrupted ${xsl_path}" >&2
  exit 1
fi

echo "[PetStore] Disabling supplier TPA SupplierOrder DTD validation"
echo "[PetStore] Patching supplier TPA line item XSL namespace attributes"
(cd module && jar uf "../${module}" \
  META-INF/ejb-jar.xml \
  com/sun/j2ee/blueprints/supplier/rsrc/xsl/TPASupplierOrder.xsl)

jar uf "${ear_path}" "${module}"
