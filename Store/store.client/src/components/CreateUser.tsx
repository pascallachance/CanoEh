import { CreateUser as SharedCreateUser } from 'canoeh-shared-ui';
import 'canoeh-shared-ui/CreateUser.css';

interface CreateUserProps {
    onCreateSuccess?: () => void;
}

function CreateUser({ onCreateSuccess }: CreateUserProps) {
    return (
        <SharedCreateUser
            title="CanoEh!"
            apiBaseUrl={import.meta.env.VITE_API_STORE_BASE_URL}
            onCreateSuccess={onCreateSuccess}
        />
    );
}

export default CreateUser;