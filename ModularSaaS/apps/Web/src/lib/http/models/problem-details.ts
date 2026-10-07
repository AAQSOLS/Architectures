/**
 * RFC 7807 Problem Details representation matching the ASP.NET Core backend.
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  extensions?: Record<string, unknown>;
}

export class HttpError extends Error {
  readonly status: number;
  readonly problemDetails: ProblemDetails;

  constructor(problemDetails: ProblemDetails) {
    super(problemDetails.detail ?? problemDetails.title);
    this.name = "HttpError";
    this.status = problemDetails.status;
    this.problemDetails = problemDetails;
  }

  get isValidation(): boolean {
    return this.status === 400 || this.status === 422;
  }

  get isUnauthorized(): boolean {
    return this.status === 401;
  }

  get isForbidden(): boolean {
    return this.status === 403;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }

  get isConflict(): boolean {
    return this.status === 409;
  }

  get isServer(): boolean {
    return this.status >= 500;
  }

  getFirstError(field?: string): string | undefined {
    if (!this.problemDetails.errors) {
      return this.problemDetails.detail ?? this.problemDetails.title;
    }

    if (field && this.problemDetails.errors[field]?.length) {
      return this.problemDetails.errors[field][0];
    }

    const firstField = Object.keys(this.problemDetails.errors)[0];
    if (firstField && this.problemDetails.errors[firstField]?.length) {
      return this.problemDetails.errors[firstField][0];
    }

    return this.problemDetails.detail ?? this.problemDetails.title;
  }
}
