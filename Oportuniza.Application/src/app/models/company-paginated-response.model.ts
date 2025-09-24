import { CompanyListDto } from "./company-list-dto-model";

export interface CompanyPaginatedResponse {
  items: CompanyListDto[];
  totalItems: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}
