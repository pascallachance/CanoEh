import React from 'react';

interface FlagIconProps {
    language: 'en' | 'fr';
    className?: string;
}

const flagStyles: React.CSSProperties = {
    display: 'inline-block',
    verticalAlign: 'middle',
    marginRight: '6px'
};

// Fleur-de-lis path for Quebec flag
const fleurDeLisPath = "M 5,0 L 6,4 L 8,3 L 7,6 L 10,7 L 7,8 L 8,11 L 6,10 L 5,14 L 4,10 L 2,11 L 3,8 L 0,7 L 3,6 L 2,3 L 4,4 Z";

// Maple leaf path for Canadian flag - based on official Flag of Canada design
// 11-pointed maple leaf with authentic proportions and characteristic shape
const mapleLeafPath = "M 10,3.5 L 10.15,4.9 L 11.4,4.5 L 10.65,5.6 L 12.1,6.4 L 10.75,6.95 L 11.6,8.5 L 10.35,7.8 L 10.7,9.3 L 10,8.5 L 9.3,9.3 L 9.65,7.8 L 8.4,8.5 L 9.25,6.95 L 7.9,6.4 L 9.35,5.6 L 8.6,4.5 L 9.85,4.9 Z";

function FlagIcon({ language, className = '' }: FlagIconProps) {
    const ariaLabel = language === 'en' ? 'English' : 'Quebec French';
    
    if (language === 'en') {
        // Canadian Flag (simplified)
        return (
            <svg
                className={className}
                width="24"
                height="18"
                viewBox="0 0 20 15"
                xmlns="http://www.w3.org/2000/svg"
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            >
                {/* Red bars on left and right */}
                <rect width="5" height="15" x="0" fill="#FF0000" />
                <rect width="5" height="15" x="15" fill="#FF0000" />
                {/* White center */}
                <rect width="10" height="15" x="5" fill="#FFFFFF" />
                {/* Maple leaf in center with stem */}
                <path d={mapleLeafPath} fill="#FF0000" />
                <rect width="0.55" height="2.7" x="9.725" y="9.3" fill="#FF0000" />
            </svg>
        );
    } else {
        // Quebec Flag
        return (
            <svg
                className={className}
                width="24"
                height="18"
                viewBox="0 0 20 15"
                xmlns="http://www.w3.org/2000/svg"
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            >
                {/* Blue background */}
                <rect width="20" height="15" fill="#003F87" />
                {/* White cross */}
                <rect width="20" height="3" y="6" fill="#FFFFFF" />
                <rect width="3" height="15" x="8.5" fill="#FFFFFF" />
                {/* Four white fleur-de-lis in corners */}
                {/* Top-left fleur-de-lis */}
                <g transform="translate(3, 2) scale(0.35)">
                    <path d={fleurDeLisPath} fill="#FFFFFF" />
                </g>
                {/* Top-right fleur-de-lis */}
                <g transform="translate(14, 2) scale(0.35)">
                    <path d={fleurDeLisPath} fill="#FFFFFF" />
                </g>
                {/* Bottom-left fleur-de-lis */}
                <g transform="translate(3, 10) scale(0.35)">
                    <path d={fleurDeLisPath} fill="#FFFFFF" />
                </g>
                {/* Bottom-right fleur-de-lis */}
                <g transform="translate(14, 10) scale(0.35)">
                    <path d={fleurDeLisPath} fill="#FFFFFF" />
                </g>
            </svg>
        );
    }
}

export default FlagIcon;
