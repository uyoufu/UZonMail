import LoadAsset from '../forTest/LoadPublicAsset.vue'

describe('load assets', () => {
  it('verifies that the image is loaded and rendered correctly', () => {
    cy.mount(LoadAsset)
    cy.dataCy('test-image')
      .should('be.visible')
      .and(($img) => {
        expect(($img[0] as HTMLImageElement).naturalWidth).to.be.greaterThan(0)
      })
  })
})

