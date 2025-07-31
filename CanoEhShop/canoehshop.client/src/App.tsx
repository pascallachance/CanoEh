import { useEffect, useState } from 'react';
import './App.css';
import Login from './components/Login';

interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();
    const [error, setError] = useState<string | null>(null);
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    useEffect(() => {
        // Check if user is already logged in
        const token = localStorage.getItem('authToken');
        if (token) {
            setIsLoggedIn(true);
            populateWeatherData();
        }
    }, []);

    const contents = forecasts === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
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

    const handleLogout = () => {
        localStorage.removeItem('authToken');
        localStorage.removeItem('sessionId');
        setIsLoggedIn(false);
        setForecasts(undefined);
    };

    const handleLoginSuccess = () => {
        setIsLoggedIn(true);
        populateWeatherData();
    };

    if (!isLoggedIn) {
        return <Login onLoginSuccess={handleLoginSuccess} />;
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <h1 id="tableLabel">Weather forecast</h1>
                <button onClick={handleLogout} style={{ padding: '0.5rem 1rem', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    Logout
                </button>
            </div>
            <p>This component demonstrates fetching data from the server.</p>
            {error && <p style={{ color: 'red' }}>Error: {error}</p>}
            {contents}
        </div>
    );

    async function populateWeatherData() {
        try {
            const response = await fetch('weatherforecast');
            if (response.ok) {
                const data = await response.json();
                setForecasts(data);
                setError(null); // Clear any previous errors
            } else {
                setError(`Failed to fetch data: ${response.status} ${response.statusText}`);
            }
        } catch (err) {
            setError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
        }
    }
}

export default App;