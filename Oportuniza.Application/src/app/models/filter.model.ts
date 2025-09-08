export interface PublicationFilterDto {
  searchTerm?: string;
  local?: string;
  contracts?: string[];
  shifts?: string[];
  salaryRange?: string | null;
}
