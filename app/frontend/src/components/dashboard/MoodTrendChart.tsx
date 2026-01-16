import React, { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box,
  Fade,
  Paper,
  Popper,
  Typography,
  useTheme,
} from '@mui/material';

import type { ChartDataPoint } from '@/utils/moodTrendUtils';

interface MoodTrendChartProps {
  chartData: ChartDataPoint[];
  showUserTrend?: boolean;
}

const MoodTrendChart: React.FC<MoodTrendChartProps> = ({
  chartData,
  showUserTrend = true,
}) => {
  const { t } = useTranslation();
  const theme = useTheme();
  
  const [tooltipState, setTooltipState] = useState<{
    anchorEl: HTMLElement | null;
    data: ChartDataPoint | null;
  }>({ anchorEl: null, data: null });

  // Chart Dimensions
  const VIEWBOX_WIDTH = 100;
  const VIEWBOX_HEIGHT = 40;
  const PADDING_TOP = 5;
  const PADDING_BOTTOM = 5;
  const CHART_HEIGHT = VIEWBOX_HEIGHT - PADDING_TOP - PADDING_BOTTOM;

  const getX = (index: number) => {
    if (chartData.length <= 1) return VIEWBOX_WIDTH / 2;
    return (index / (chartData.length - 1)) * VIEWBOX_WIDTH;
  };

  const getY = (value: number | null) => {
    if (value === null) return null;
    const normalized = (value - 1) / (3 - 1); 
    return PADDING_TOP + CHART_HEIGHT - normalized * CHART_HEIGHT;
  };

  const points = chartData.map((d, i) => {
    const y = getY(d.average);
    const userY = getY(d.userAverage);
    return { x: getX(i), y, userY, data: d };
  });

  const validPoints = points.filter((p) => p.y !== null) as {
    x: number;
    y: number;
    userY: number | null;
    data: ChartDataPoint;
  }[];

  const validUserPoints = points.filter((p) => p.userY !== null) as {
    x: number;
    y: number | null;
    userY: number;
    data: ChartDataPoint;
  }[];

  const getCurvePath = (dataPoints: { x: number; y: number }[]) => {
    if (dataPoints.length === 0) return '';
    if (dataPoints.length === 1) return `M ${dataPoints[0].x} ${dataPoints[0].y}`;

    let path = `M ${dataPoints[0].x} ${dataPoints[0].y}`;

    for (let i = 0; i < dataPoints.length - 1; i++) {
      const p0 = dataPoints[i];
      const p1 = dataPoints[i + 1];
      const cp1x = p0.x + (p1.x - p0.x) / 2;
      const cp1y = p0.y;
      const cp2x = p0.x + (p1.x - p0.x) / 2;
      const cp2y = p1.y;

      path += ` C ${cp1x} ${cp1y}, ${cp2x} ${cp2y}, ${p1.x} ${p1.y}`;
    }
    return path;
  };

  const linePath = useMemo(() => getCurvePath(validPoints), [validPoints]);
  
  const userLinePath = useMemo(() => 
    getCurvePath(validUserPoints.map(p => ({ x: p.x, y: p.userY }))), 
    [validUserPoints]
  );

  const areaPath = useMemo(() => {
    if (validPoints.length === 0) return '';
    const first = validPoints[0];
    const last = validPoints[validPoints.length - 1];
    return `${linePath} L ${last.x} ${VIEWBOX_HEIGHT} L ${first.x} ${VIEWBOX_HEIGHT} Z`;
  }, [linePath, validPoints]);

  const userAreaPath = useMemo(() => {
    if (validUserPoints.length === 0) return '';
    const first = validUserPoints[0];
    const last = validUserPoints[validUserPoints.length - 1];
    return `${userLinePath} L ${last.x} ${VIEWBOX_HEIGHT} L ${first.x} ${VIEWBOX_HEIGHT} Z`;
  }, [userLinePath, validUserPoints]);

  return (
    <Box sx={{ flexGrow: 1, height: 100, mt: 1, position: 'relative', ml: 5 }}>
      {chartData.length > 0 ? (
        <>
          <svg
            viewBox={`0 0 ${VIEWBOX_WIDTH} ${VIEWBOX_HEIGHT}`}
            style={{ width: '100%', height: '100%', overflow: 'visible', display: 'block' }}
            preserveAspectRatio="none"
          >
            <defs>
              <linearGradient id="gradientMood" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor={theme.palette.primary.main} stopOpacity={0.6} />
                <stop offset="100%" stopColor={theme.palette.primary.main} stopOpacity={0} />
              </linearGradient>
              <linearGradient id="gradientUserMood" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor={theme.palette.secondary.main} stopOpacity={0.4} />
                <stop offset="100%" stopColor={theme.palette.secondary.main} stopOpacity={0} />
              </linearGradient>
            </defs>

            {[1, 2, 3].map((val) => {
              const y = getY(val);
              return (
                y !== null && (
                  <line
                    key={val}
                    x1="0"
                    y1={y}
                    x2={VIEWBOX_WIDTH}
                    y2={y}
                    stroke={theme.palette.divider}
                    strokeWidth="0.1"
                    strokeDasharray="1 1"
                  />
                )
              );
            })}

            <path d={areaPath} fill="url(#gradientMood)" />

            <path
              d={linePath}
              fill="none"
              stroke={theme.palette.primary.main}
              strokeWidth="0.6"
              strokeLinecap="round"
              strokeLinejoin="round"
            />

            {showUserTrend && (
              <>
                <path d={userAreaPath} fill="url(#gradientUserMood)" />
                <path
                  d={userLinePath}
                  fill="none"
                  stroke={theme.palette.secondary.main}
                  strokeWidth="0.6"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeDasharray="2 1"
                />
              </>
            )}
          </svg>

          {[3, 2, 1].map((val) => {
              const y = getY(val);
              const topPercent = y !== null ? (y / VIEWBOX_HEIGHT) * 100 : 0;
              const icon = val === 3 ? '😊' : val === 2 ? '😐' : '☹️';
              
              return (
                <Typography
                  key={val}
                  variant="caption"
                  sx={{
                    position: 'absolute',
                    left: -32,
                    top: `${topPercent}%`,
                    transform: 'translateY(-50%)',
                    fontSize: '1.2rem',
                    lineHeight: 1,
                    zIndex: 1,
                  }}
                >
                  {icon}
                </Typography>
              );
          })}

          {validPoints.map((p, i) => {
              const leftPercent = (p.x / VIEWBOX_WIDTH) * 100;
              const topPercent = (p.y / VIEWBOX_HEIGHT) * 100;

              return (
                <Box
                  key={`dot-${i}`}
                  sx={{
                    position: 'absolute',
                    left: `${leftPercent}%`,
                    top: `${topPercent}%`,
                    width: 16,
                    height: 16,
                    borderRadius: '50%',
                    backgroundColor: theme.palette.background.paper,
                    border: `2px solid ${theme.palette.primary.main}`,
                    transform: 'translate(-50%, -50%)',
                    cursor: 'pointer',
                    zIndex: 2,
                    transition: 'transform 0.2s, box-shadow 0.2s',
                    '&:hover': {
                       transform: 'translate(-50%, -50%) scale(1.25)',
                       boxShadow: theme.shadows[3],
                       zIndex: 3,
                    }
                  }}
                  onMouseEnter={(e) => setTooltipState({ anchorEl: e.currentTarget, data: p.data })}
                  onMouseLeave={() => setTooltipState({ anchorEl: null, data: null })}
                />
              );
          })}

          {showUserTrend && validUserPoints.map((p, i) => {
              const leftPercent = (p.x / VIEWBOX_WIDTH) * 100;
              const topPercent = (p.userY / VIEWBOX_HEIGHT) * 100;

              return (
                <Box
                  key={`user-dot-${i}`}
                  sx={{
                    position: 'absolute',
                    left: `${leftPercent}%`,
                    top: `${topPercent}%`,
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    backgroundColor: theme.palette.secondary.main,
                    border: `2px solid ${theme.palette.background.paper}`,
                    transform: 'translate(-50%, -50%)',
                    cursor: 'pointer',
                    zIndex: 4,
                    transition: 'transform 0.2s, box-shadow 0.2s',
                    '&:hover': {
                       transform: 'translate(-50%, -50%) scale(1.25)',
                       boxShadow: theme.shadows[3],
                       zIndex: 5,
                    }
                  }}
                  onMouseEnter={(e) => setTooltipState({ anchorEl: e.currentTarget, data: p.data })}
                  onMouseLeave={() => setTooltipState({ anchorEl: null, data: null })}
                />
              );
          })}

          <Popper 
            open={Boolean(tooltipState.anchorEl)} 
            anchorEl={tooltipState.anchorEl} 
            placement="top" 
            transition
            modifiers={[
              {
                name: 'offset',
                options: {
                  offset: [0, 8],
                },
              },
            ]}
            style={{ zIndex: 1500 }}
          >
            {({ TransitionProps }) => (
              <Fade {...TransitionProps} timeout={200}>
                <Paper
                  sx={{
                    p: 1,
                    border: `1px solid ${theme.palette.divider}`,
                    backgroundColor: theme.palette.background.paper,
                    boxShadow: theme.shadows[3],
                  }}
                >
                   <Typography variant="caption" display="block" color="text.secondary">
                    {t('common.date')}: {tooltipState.data?.displayDate}
                  </Typography>
                  {tooltipState.data?.average !== null && (
                    <Typography variant="body2" fontWeight="bold" color="primary.main">
                      {t('dashboard.averageMood')}: {tooltipState.data?.average}
                    </Typography>
                  )}
                  {showUserTrend && tooltipState.data?.userAverage !== null && (
                    <Typography variant="body2" fontWeight="bold" color="secondary.main">
                      {t('dashboard.yourMood', 'Votre humeur')}: {tooltipState.data?.userAverage}
                    </Typography>
                  )}
                </Paper>
              </Fade>
            )}
          </Popper>
        </>
      ) : (
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            height: '100%',
          }}
        >
          <Typography color="text.secondary">{t('common.noData')}</Typography>
        </Box>
      )}
    </Box>
  );
};

export default MoodTrendChart;
