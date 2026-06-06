import { Dark } from 'quasar'
import QuasarDark from '../forTest/QuasarDark.vue'

describe('QuasarDark', () => {
  it('changes its color', () => {
    cy.mount(QuasarDark)

    cy.dataCy('dark-card')
      .should('not.have.class', 'q-dark')
      .then(() => {
        Dark.set(true)
      })

    cy.dataCy('dark-card')
      .should('have.class', 'q-dark')
      .then(() => {
        Dark.set(false)
      })

    cy.dataCy('dark-card').should('not.have.class', 'q-dark')
  })
})

