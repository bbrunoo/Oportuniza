import { Publication } from "./Publications.model";

export interface CandidateDTO {
  id: string;
  publicationId: string;
  publicationTitle: string;
  userId: string;
  userName: string;
  userIdKeycloak: string;
  applicationDate: string;
  userImage: string;
  status: string;
}

export interface PublicationWithCandidates {
  publicationId: string;
  title: string;
  resumee: string;
  authorImageUrl: string;
  name: string;
  candidates: CandidateDTO[];
}

export interface CreateCandidatura {
  publicationId: string;
}

export interface Candidatura {
  id: string;
  publicationId: string;
  publicationTitle: string;
  userId: string;
  userName: string;
  userIdKeycloak: string;
  applicationDate: string;
  status: CandidateApplicationStatus;
}

export enum CandidateApplicationStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2
}

export interface UserApplication {
  id: string;
  publication: Publication;
}
