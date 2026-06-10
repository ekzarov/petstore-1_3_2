export interface SignInRequest {
  userId: string;
  password: string;
}

export interface SignInResponse {
  token: string;
  userId: string;
  role: string;
  expiresAt: string;
}

export interface ContactInfo {
  familyName: string;
  givenName: string;
  street1: string;
  street2?: string | null;
  city: string;
  state: string;
  zip: string;
  country: string;
  email: string;
  phone: string;
}

export interface RegisterRequest {
  userId: string;
  password: string;
  contact: ContactInfo;
}

export interface Account {
  userId: string;
  contact: ContactInfo | null;
}

export interface ApiError {
  code: string;
  message: string;
}
