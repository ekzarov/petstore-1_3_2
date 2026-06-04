FROM payara/server-full:5.2021.1

COPY --chown=payara:payara docker/payara-petstore/init_0_patch_h2_cmp.sh /opt/payara/scripts/init_0_patch_h2_cmp.sh
COPY --chown=payara:payara docker/payara-petstore/init_0_create_jdbc_resources.sh /opt/payara/scripts/init_0_create_jdbc_resources.sh
COPY --chown=payara:payara docker/payara-petstore/patch_ear_cmp.sh /opt/payara/patch_ear_cmp.sh
COPY --chown=payara:payara docker/payara-petstore/descriptors /opt/payara/petstore-descriptors
COPY --chown=payara:payara petstore.ear /opt/payara/deployments/petstore.ear
COPY --chown=payara:payara opc.ear /opt/payara/petstore-ears/opc.ear
COPY --chown=payara:payara supplier.ear /opt/payara/petstore-ears/supplier.ear

RUN chmod +x /opt/payara/scripts/init_0_patch_h2_cmp.sh \
    /opt/payara/scripts/init_0_create_jdbc_resources.sh \
    /opt/payara/patch_ear_cmp.sh \
  && /opt/payara/patch_ear_cmp.sh /opt/payara/deployments/petstore.ear jdbc/petstore/PetStoreDB \
    customer-ejb.jar signon-ejb.jar uidgen-ejb.jar \
  && /opt/payara/patch_ear_cmp.sh /opt/payara/petstore-ears/opc.ear jdbc/opc/OPCDB \
    opc-ejb.jar po-ejb.jar processmanager-ejb.jar mailer-ejb.jar \
  && /opt/payara/patch_ear_cmp.sh /opt/payara/petstore-ears/supplier.ear jdbc/supplier/SupplierDB \
    supplier-ejb.jar supplierpo-ejb.jar processmanager-ejb.jar
