import { Login as SharedLogin } from 'canoeh-shared-ui';
import 'canoeh-shared-ui/Login.css';

interface LoginProps {
    onLoginSuccess?: () => void;
}

function Login({ onLoginSuccess }: LoginProps) {
    return (
        <SharedLogin
            title="CanoEh!"
            apiBaseUrl={import.meta.env.VITE_API_STORE_BASE_URL}
            onLoginSuccess={onLoginSuccess}
            enableEscapeKeyHandling={true}
        />
    );
}

export default Login;