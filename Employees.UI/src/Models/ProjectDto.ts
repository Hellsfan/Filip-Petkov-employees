export interface ProjectDto {
  id: string
  employeeId: number
  name: number
  from : string
  to : string
}

// src/models/PagedResult.ts
export interface PagedResult<T> {
  items: T[]  
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}