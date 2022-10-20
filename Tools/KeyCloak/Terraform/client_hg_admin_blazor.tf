resource "keycloak_openid_client" "hgadminblazor_client" {
  realm_id                     = var.keycloak_realm
  client_id                    = "HealthGatewayAdminBlazor"
  name                         = "Health Gateway Admin Blazor"
  description                  = "Health Gateway Blazor Admin web application"
  enabled                      = true
  access_type                  = "PUBLIC"
  login_theme                  = "bcgov-idp-stopper"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  valid_redirect_uris          = var.client_hg_admin_blazor_valid_redirects
  web_origins                  = var.client_hg_admin_blazor_web_origins
  full_scope_allowed           = false
}

resource "keycloak_generic_role_mapper" "hgadminblazor_adminreviewer" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadminblazor_client.id
  role_id   = keycloak_role.AdminReviewer.id
}

resource "keycloak_generic_role_mapper" "hgadminblazor_adminuser" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadminblazor_client.id
  role_id   = keycloak_role.AdminUser.id
}

resource "keycloak_generic_role_mapper" "hgadminblazor_supportuser" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadminblazor_client.id
  role_id   = keycloak_role.SupportUser.id
}