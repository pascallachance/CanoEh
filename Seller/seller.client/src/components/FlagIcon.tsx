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
                {/* Simplified maple leaf in center */}
                <path d="M 10,4 L 10.5,6 L 12,6.5 L 10.5,7 L 10.8,8.5 L 10,7.5 L 9.2,8.5 L 9.5,7 L 8,6.5 L 9.5,6 Z" fill="#FF0000" />
                <rect width="0.8" height="3" x="9.6" y="8" fill="#FF0000" />
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
