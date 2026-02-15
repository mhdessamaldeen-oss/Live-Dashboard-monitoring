export interface Result<T> {
    isSuccess: boolean;
    data: T;
    message: string | null;
    errors: string[];
}
