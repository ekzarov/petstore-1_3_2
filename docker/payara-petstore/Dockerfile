FROM payara/server-full:5.2021.1

COPY --chown=payara:payara docker/payara-petstore/init_0_patch_h2_cmp.sh /opt/payara/scripts/init_0_patch_h2_cmp.sh
COPY --chown=payara:payara petstore.ear /opt/payara/deployments/petstore.ear

RUN chmod +x /opt/payara/scripts/init_0_patch_h2_cmp.sh
