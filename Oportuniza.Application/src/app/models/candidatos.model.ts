export interface Candidato {
  id: string;                 // GUID da candidatura
  publicationId: string;      // GUID da publicação
  userId: string;             // GUID do usuário
  userName: string;           // Nome do candidato
  status: number;             // Status da candidatura
  applicationDate: string;
}

export interface Candidatura {
  id: string;                // GUID da candidatura (equivalente a applicationId)
  publicationId: string;     // GUID da publicação
  logoEmpresaUrl: string;
  nomeEmpresa: string;
  tituloVaga: string;
  showModal?: boolean;
}

export interface PublicationWithCandidates {
  publicationId: string;
  title: string;
  candidates: Candidato[];
}

export interface HasAppliedResponse {
  hasApplied: boolean;
}
