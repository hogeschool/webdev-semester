import { LoaderFunctionArgs, json, redirect } from "react-router-dom"


interface DataFetchOptions<T> {
    fallbackRedirectUrl: string
    redirectCondition: (response: ResponseWrapper<T>) => boolean
}

export const GenericDataFetch = <T>(fetch: (args: LoaderFunctionArgs<any>) => Promise<ResponseWrapper<T>>, options?: DataFetchOptions<T>) =>
    async (args: LoaderFunctionArgs<any>): Promise<Response> => {
        const response = await fetch(args)
        if (options?.redirectCondition(response)) return redirect(options.fallbackRedirectUrl)
        return json(response)
    }

export interface ResponseWrapper<T> {
    status: number
    response: T
}

export const IsAdminLoggedIn = async (args?: LoaderFunctionArgs<any>): Promise<ResponseWrapper<string>> => {
    const response = await fetch(`/api/v1/Login/IsAdminLoggedIn`)
    const msg = await response.text()
    return <ResponseWrapper<string>>({ status: response.status, response: msg })
}

export const LoginAsAdmin = async (args?: LoaderFunctionArgs<any>): Promise<ResponseWrapper<string>> => {
    if (!args?.request) return <ResponseWrapper<string>>({ status: 403, response: "Invalid arguments" })
    const data = Object.fromEntries(await args.request.formData());
    const json_body = JSON.stringify({ Username: data.Username, Password: data.Password })
    const response = await fetch(`/api/v1/Login/Login`, {
        method: 'post',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: json_body
    })
    const msg = await response.text()
    return <ResponseWrapper<string>>({ status: response.status, response: msg })
}