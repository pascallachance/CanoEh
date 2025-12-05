import { Login as SharedLogin } from 'canoeh-shared-ui';
import 'canoeh-shared-ui/Login.css';

interface LoginProps {
    onLoginSuccess?: () => void;
}

function Login({ onLoginSuccess }: LoginProps) {
    return (
        <SharedLogin
            title="CanoEh! Seller"
            apiBaseUrl={import.meta.env.VITE_API_SELLER_BASE_URL}
            onLoginSuccess={onLoginSuccess}
        />
    );
}

export default Login;