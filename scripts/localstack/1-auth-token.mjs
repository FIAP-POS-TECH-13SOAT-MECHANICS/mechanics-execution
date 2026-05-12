const SUB = '8e0fe84e-ab27-43ee-bc0d-e75e88d14cf6';
const SECRET = 'a3f8c2e1d94b567f0e2a1c8d3b6f9e4a7c0d2f5b8e1a4c7d0f3b6e9a2c5d8f1';

function base64url(value) {
    return Buffer.from(JSON.stringify(value))
        .toString('base64')
        .replace(/=/g, '')
        .replace(/\+/g, '-')
        .replace(/\//g, '_');
}

export const handler = async () => {
    const now = Math.floor(Date.now() / 1000);
    const exp = now + 180;

    const header = base64url({alg: 'HS256', typ: 'JWT'});
    const payload = base64url({
        iss: 'fiap-mechanics',
        iat: now,
        nbf: now,
        exp: exp,
        sub: SUB,
        customerId: '00000000-0000-0000-0000-000000000000',
        role: 'SERVICE',
    });

    const token = `${header}.${payload}.${base64url(SECRET)}`;

    return {
        statusCode: 200,
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({
            accessToken: token,
            refreshToken: '',
            expiresIn: 180,
            expirationDate: new Date(exp * 1000).toISOString(),
        }),
        isBase64Encoded: false,
    };
};
