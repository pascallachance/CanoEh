import { useEffect, useState } from 'react';
import './App.css';
import Login from './components/Login';

interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

interface AuthStatus {
    isAuthenticated: boolean;
    username?: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();
    const [authStatus, setAuthStatus] = useState<AuthStatus>({ isAuthenticated: false });
    const [authLoading, setAuthLoading] = useState(true);

    useEffect(() => {
        checkAuthStatus();
    }, []);

    useEffect(() => {
        if (authStatus.isAuthenticated) {
            populateWeatherData();
        }
    }, [authStatus.isAuthenticated]);

    const checkAuthStatus = async () => {
        try {
            const response = await fetch('/api/store/demologin/status', {
                credentials: 'include', // Include cookies
            });
            
            if (response.ok) {
                const data: AuthStatus = await response.json();
                setAuthStatus(data);
            } else {
                setAuthStatus({ isAuthenticated: false });
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            setAuthStatus({ isAuthenticated: false });
        } finally {
            setAuthLoading(false);
        }
    };

    const handleLoginSuccess = () => {
        checkAuthStatus(); // Refresh auth status after successful login
    };

    const handleLogout = async () => {
        try {
            const response = await fetch('/api/store/demologin/logout', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            
            if (response.ok) {
                setAuthStatus({ isAuthenticated: false });
                setForecasts(undefined);
            }
        } catch (error) {
            console.error('Logout error:', error);
        }
    };

    async function populateWeatherData() {
        try {
            const response = await fetch('weatherforecast', {
                credentials: 'include', // Include cookies for authenticated requests
            });
            if (response.ok) {
                const data = await response.json();
                setForecasts(data);
            } else {
                console.error(`Failed to fetch weather data: ${response.status} ${response.statusText}`);
                if (response.status === 401) {
                    // Token expired or invalid, update auth status
                    setAuthStatus({ isAuthenticated: false });
                } else {
                    alert('Failed to fetch weather data. Please try again later.');
                }
            }
        } catch (error) {
            console.error('An error occurred while fetching weather data:', error);
            alert('An error occurred while fetching weather data. Please check your network connection and try again.');
        }
    }

    // Show loading spinner while checking authentication
    if (authLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <p>Loading...</p>
            </div>
        );
    }

    // Show login form if not authenticated
    if (!authStatus.isAuthenticated) {
        return <Login onLoginSuccess={handleLoginSuccess} />;
    }

    // Show main app content if authenticated
    const contents = forecasts === undefined
        ? <p><em>Loading weather data...</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>
            <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.date}>
                        <td>{forecast.date}</td>
                        <td>{forecast.temperatureC}</td>
                        <td>{forecast.temperatureF}</td>
                        <td>{forecast.summary}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <header style={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center', 
                padding: '1rem',
                borderBottom: '1px solid #eee'
            }}>
                <h1 id="tableLabel">Weather forecast</h1>
                <div>
                    <span>Welcome, {authStatus.username || 'User'}!</span>
                    <button 
                        onClick={handleLogout}
                        style={{ 
                            marginLeft: '1rem',
                            padding: '0.5rem 1rem',
                            background: '#dc3545',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Logout
                    </button>
                </div>
            </header>
            <main style={{ padding: '1rem' }}>
                <p>This component demonstrates fetching data from the server with secure authentication.</p>
                {contents}
            </main>
        </div>
    );
}

export default App;