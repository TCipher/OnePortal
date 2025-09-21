window.webauthn = {
    get: async function (publicKeyOptions) {
        publicKeyOptions.challenge = Uint8Array.from(atob(publicKeyOptions.challenge), c => c.charCodeAt(0));
        const cred = await navigator.credentials.get({ publicKey: publicKeyOptions });
        return {
            id: cred.id,
            rawId: btoa(String.fromCharCode(...new Uint8Array(cred.rawId))),
            type: cred.type,
            response: {
                clientDataJSON: btoa(String.fromCharCode(...new Uint8Array(cred.response.clientDataJSON))),
                authenticatorData: btoa(String.fromCharCode(...new Uint8Array(cred.response.authenticatorData))),
                signature: btoa(String.fromCharCode(...new Uint8Array(cred.response.signature))),
                userHandle: cred.response.userHandle ? btoa(String.fromCharCode(...new Uint8Array(cred.response.userHandle))) : null
            }
        };
    }
}