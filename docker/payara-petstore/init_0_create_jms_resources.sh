#!/bin/bash
set -eu

touch "${POSTBOOT_COMMANDS}"

append_once() {
  command="$1"
  marker="$2"

  if grep -q "${marker}" "${POSTBOOT_COMMANDS}"; then
    echo "[PetStore] JMS command already present: ${marker}"
    return
  fi

  echo "${command}" >> "${POSTBOOT_COMMANDS}"
}

create_destination() {
  jndi_name="$1"
  destination_type="$2"
  physical_name="${jndi_name//\//_}"

  append_once \
    "create-jmsdest --force=true --desttype ${destination_type} ${physical_name}" \
    "jmsdest ${physical_name}"

  append_once \
    "create-jms-resource --force=true --restype javax.jms.${destination_type^} --property Name=${physical_name} ${jndi_name}" \
    "jms-resource ${jndi_name}"
}

create_connection_factory() {
  jndi_name="$1"
  factory_type="$2"

  append_once \
    "create-jms-resource --force=true --restype javax.jms.${factory_type} ${jndi_name}" \
    "jms-resource ${jndi_name}"
}

create_mail_session() {
  jndi_name="$1"

  append_once \
    "create-javamail-resource --mailhost localhost --mailuser petstore --fromaddress petstore@example.test ${jndi_name}" \
    "javamail-resource ${jndi_name}"
}

create_destination "jms/opc/OrderQueue" "queue"
create_destination "jms/opc/OrderApprovalQueue" "queue"
create_destination "jms/opc/MailOrderApprovalQueue" "queue"
create_destination "jms/opc/MailCompletedOrderQueue" "queue"
create_destination "jms/opc/MailQueue" "queue"
create_destination "jms/supplier/PurchaseOrderQueue" "queue"
create_destination "jms/opc/InvoiceTopic" "topic"

create_connection_factory "jms/opc/QueueConnectionFactory" "QueueConnectionFactory"
create_connection_factory "jms/opc/TopicConnectionFactory" "TopicConnectionFactory"
create_connection_factory "jms/supplier/QueueConnectionFactory" "QueueConnectionFactory"
create_connection_factory "jms/supplier/TopicConnectionFactory" "TopicConnectionFactory"
create_connection_factory "jms/petstore/QueueConnectionFactory" "QueueConnectionFactory"
create_connection_factory "jms/admin/QueueConnectionFactory" "QueueConnectionFactory"

create_mail_session "mail/opc/MailSession"
