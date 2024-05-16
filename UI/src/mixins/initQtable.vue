<script>
// 快速导入表格组件配置
// 使用需知：
// 1. 在data中已经定义了如下属性，请不要在组件中重复定义用作它用

export default {
  data() {
    return {
      filter: '',
      loading: false,
      pagination: {
        sortBy: '_id',
        descending: true,
        page: 1,
        rowsPerPage: 15,
        rowsNumber: 0
      },
      data: []
    }
  },

  computed: {
    // Computed property to compute filter object
    initQuasarTable_computed_filterObj() {
      return this.initQuasarTable_createFilter(this.filter)
    },

    // Computed property to compute number of rows
    initQuasarTable_computed_rowsNumber() {
      return this.initQuasarTable_getFilterCount(this.initQuasarTable_computed_filterObj)
    }
  },

  // Lifecycle hooks
  async activated() {
    this.initQuasarTable_onRequest({
      pagination: this.pagination
    })
  },

  async mounted() {
    this.initQuasarTable_onRequest({
      pagination: this.pagination
    })
  },

  // Methods
  methods: {
    async initQuasarTable_getFilterCount(filterObj) {
      return 0
    },

    async initQuasarTable_getFilterList(filterObj, pagination) {
      return []
    },

    async initQuasarTable_resultHandler(data) {
      return data
    },

    initQuasarTable_createFilter(filter) {
      return {
        filter
      }
    },

    async initQuasarTable_onRequest(props) {
      let paginationParams = this.pagination
      if (props) {
        paginationParams = props.pagination
      }

      const { page, rowsPerPage, sortBy, descending } = paginationParams

      this.loading = true

      const rowsCount = await this.initQuasarTable_getFilterCount(this.initQuasarTable_computed_filterObj)

      const fetchCount = rowsPerPage === 0 ? rowsCount : rowsPerPage

      if (fetchCount === 0) {
        this.loading = false
        this.data = []
        return
      }

      const startRow = (page - 1) * rowsPerPage

      const filterObj = this.initQuasarTable_computed_filterObj

      const pagination = {
        sortBy,
        descending,
        skip: startRow,
        limit: fetchCount
      }

      const returnedData = await this.initQuasarTable_getFilterList(filterObj, pagination)

      this.data = await this.initQuasarTable_resultHandler(returnedData)

      this.pagination.page = page
      this.pagination.rowsPerPage = rowsPerPage
      this.pagination.sortBy = sortBy
      this.pagination.descending = descending
      this.pagination.rowsNumber = rowsCount

      this.loading = false
    }
  }
}
</script>
