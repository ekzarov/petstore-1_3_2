#!/bin/bash
set -eu

ear_path="$1"
work_dir="$(mktemp -d)"
class_path="com/sun/j2ee/blueprints/xmldocuments/tpa/TPALineItemUtils.class"

cleanup() {
  rm -rf "${work_dir}"
}
trap cleanup EXIT

cd "${work_dir}"
jar xf "${ear_path}"

mkdir -p src/com/sun/j2ee/blueprints/xmldocuments/tpa classes
cat > src/com/sun/j2ee/blueprints/xmldocuments/tpa/TPALineItemUtils.java <<'JAVA'
package com.sun.j2ee.blueprints.xmldocuments.tpa;

import org.w3c.dom.*;

public class TPALineItemUtils {
  public static final String XML_NAMESPACE = "http://blueprints.j2ee.sun.com/TPALineItem";
  public static final String XML_PREFIX = "tpali";
  public static final String XML_LINEITEM = XML_PREFIX + ":" + "LineItem";
  public static final String XML_CATEGORYID = "categoryId";
  public static final String XML_PRODUCTID = "productId";
  public static final String XML_ITEMID = "itemId";
  public static final String XML_LINENO = "lineNo";
  public static final String XML_QUANTITY = "quantity";
  public static final String XML_UNITPRICE = "unitPrice";

  private TPALineItemUtils() {}

  public static void addLineItem(Document document, Element lineItemsElement,
                                 String categoryId, String productId, String itemId, String lineNo,
                                 int quantity, float unitPrice) {
    Element lineItemElement = document.createElementNS(XML_NAMESPACE, XML_LINEITEM);
    lineItemElement.setAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:" + XML_PREFIX, XML_NAMESPACE);
    lineItemElement.setAttribute(XML_CATEGORYID, categoryId);
    lineItemElement.setAttribute(XML_PRODUCTID, productId);
    lineItemElement.setAttribute(XML_ITEMID, itemId);
    lineItemElement.setAttribute(XML_LINENO, lineNo);
    lineItemElement.setAttribute(XML_QUANTITY, Long.toString(quantity));
    lineItemElement.setAttribute(XML_UNITPRICE, Float.toString(unitPrice));
    lineItemsElement.appendChild(lineItemElement);
    return;
  }
}
JAVA

javac -source 1.4 -target 1.4 -d classes src/com/sun/j2ee/blueprints/xmldocuments/tpa/TPALineItemUtils.java

patched_modules=""
for module in *.jar; do
  if jar tf "${module}" | grep -q "${class_path}"; then
    echo "[PetStore] Patching ${class_path} in ${module}"
    jar uf "${module}" -C classes "${class_path}"
    patched_modules="${patched_modules} ${module}"
  fi
done

if [ -z "${patched_modules}" ]; then
  echo "[PetStore] Missing ${class_path} in ${ear_path}" >&2
  exit 1
fi

jar uf "${ear_path}" ${patched_modules}
