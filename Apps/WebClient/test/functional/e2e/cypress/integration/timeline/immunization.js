const { AuthMethod } = require("../../support/constants")

describe('Immunization', () => {
    beforeEach(() => {
        cy.server();
        cy.fixture('immunizationEnabledConfig').as('config');
        cy.route('GET', '/v1/api/configuration/', '@config');
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=immunizationTitle]')
            .should('be.visible');
    })
})