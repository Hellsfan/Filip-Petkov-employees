<template>
    <h2>Projects</h2>

    <div class="import-section">
      <h3>Import Projects</h3>
      <input type="file" @change="onFileChange" />
      <button @click="uploadFile">Upload File</button>

      <div v-if="isImportMessageVisible" @click.self="closeMessage">
        <div>
          <p>{{ importMessage }}</p>
          <button @click="closeMessage">Close</button>
      </div>
    </div>

    <div class="tables-container">
      <div class="table-wrapper">
        <table border="1" cellpadding="8">
          <thead>
            <tr>
              <th>Employee Id</th>
              <th>Project Name</th>
              <th>Start Date</th>
              <th>End Date</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in projects" :key="p.id">
              <td>{{ p.employeeId }}</td>
              <td>{{ p.name }}</td>
              <td>{{ new Date(p.from).toLocaleDateString() }}</td>
              <td>{{ new Date(p.to).toLocaleDateString() }}</td>
            </tr>
          </tbody>
        </table>

        <div class="pagination">
          <button @click="prevPage" :disabled="pageNumber === 1">Previous</button>
          <span>Page {{ pageNumber }} of {{ totalPages }}</span>
          <button @click="nextPage" :disabled="pageNumber === totalPages">Next</button>
        </div>
        <div>Total projects: {{ totalCount }}</div>
      </div>

      <div class="table-wrapper">
        <table border="1" cellpadding="8">
          <thead>
            <tr>
              <th>Employee Ids</th>
              <th>Total hours worked together</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="lc in longestCommon" :key="lc.employeeIds">
              <td>{{ lc.employeeIds }}</td>
              <td>{{ lc.timeSum }}</td>
            </tr>
          </tbody>
        </table>
        <button @click="GetLongestCommon">Get Longest Common Projects</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { PagedResult, ProjectDto, LongestCommonDto } from '@/models/'

const projects = ref<ProjectDto[]>([])
const totalCount = ref(0)
const pageNumber = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)

const selectedFile = ref<File | null>(null)
const isImportMessageVisible = ref(false)
const importMessage = ref('')

const longestCommon = ref<LongestCommonDto[]>([])

async function GetLongestCommon(){
  const response = await fetch(`/api/projects/longest-common`)
  const data: LongestCommonDto[] = await response.json()

  longestCommon.value = data
}

function onFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
  }
}

// Import functions
async function uploadFile() {
  if (!selectedFile.value) return

  const formData = new FormData()
  formData.append('file', selectedFile.value)

  const response = await fetch('/api/projects/import', {
    method: 'POST',
    body: formData
  })

  if(await response.json()){
    showImportMessage("Import Successful")
  }else{
    showImportMessage("Import Failed")
  }

  selectedFile.value = ''
  loadProjects()
}

function showImportMessage(message) {
  importMessage.value = message
  isImportMessageVisible.value = true
}

function closeMessage() {
  isImportMessageVisible.value = false
}

// Main Table functions
function prevPage() {
  if (pageNumber.value > 1) {
    pageNumber.value--
    loadProjects()
  }
}

function nextPage() {
  if (pageNumber.value < totalPages.value) {
    pageNumber.value++
    loadProjects()
  }
}

async function loadProjects() {
  const response = await fetch(`/api/projects?pageNumber=${pageNumber.value}&pageSize=${pageSize.value}`)
  const data: PagedResult<ProjectDto> = await response.json()
  
  projects.value = data.items.map(p => ({
    ...p
  }))

  totalCount.value = data.totalCount
  totalPages.value = data.totalPages
}

onMounted(() => {
  loadProjects()
})
</script>

<style scoped>
.tables-container {
  display: flex;
  gap: 24px;
}

.import-section {
  margin-bottom: 20px;
}

.pagination {
  margin-top: 8px;
  margin-bottom: 8px;
}
</style>
