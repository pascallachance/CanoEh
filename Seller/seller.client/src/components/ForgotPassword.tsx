import { ForgotPassword as SharedForgotPassword } from 'canoeh-shared-ui';
import 'canoeh-shared-ui/ForgotPassword.css';
import { useNavigate } from 'react-router-dom';

function ForgotPassword() {
    const navigate = useNavigate();

    const handleSubmitSuccess = () => {
        // Navigate back to login after success
        navigate('/login');
    };

    return (
        <SharedForgotPassword
            title="CanoEh! Seller"
            apiBaseUrl={import.meta.env.VITE_API_SELLER_BASE_URL}
            onSubmitSuccess={handleSubmitSuccess}
        />
    );
}

export default ForgotPassword;
