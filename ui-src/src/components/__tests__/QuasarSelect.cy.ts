import QuasarSelect from '../forTest/QuasarSelect.vue'

function dataCySelect(dataCyId: string) {
  return cy.dataCy(dataCyId).closest('.q-select')
}

describe('QuasarSelect', () => {
  it('makes sure the select is disabled', () => {
    cy.mount(QuasarSelect, {
      props: { disable: true }
    })

    // `cy.dataCy('select')` won't work in this case, as it won't get the root q-select element
    dataCySelect('select').should('have.attr', 'aria-disabled', 'true')
  })

  it('selects an option by content', () => {
    cy.mount(QuasarSelect)

    cy.dataCy('select').select('Option 1')
    cy.dataCy('select-value').should('have.text', 'Option 1')
  })

  it('selects an option by cardinality', () => {
    cy.mount(QuasarSelect)

    cy.dataCy('select').select(1)
    cy.dataCy('select-value').should('have.text', 'Option 2')
  })

  it('selects an option asynchronously', () => {
    cy.mount(QuasarSelect, {
      props: {
        loadOptionsAsync: true
      }
    })

    // Wait for loading to complete
    cy.dataCy('select').get('.q-spinner').should('not.exist')

    cy.dataCy('select').select('Option 3')
    cy.dataCy('select-value').should('have.text', 'Option 3')
  })

  it('selects multiple options', () => {
    const options = ['Option 1', 'Option 2']

    cy.mount(QuasarSelect, {
      props: {
        multiple: true,
        options
      }
    })

    cy.dataCy('select').select(['Option 1', 'Option 2'])
  })
})

