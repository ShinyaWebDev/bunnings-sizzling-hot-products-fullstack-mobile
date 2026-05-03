import { Image, StyleSheet, Text, View } from 'react-native';

import type { SizzlingHotResult } from '@/src/types/sizzlingHot';

const flameIcon = require('@/assets/images/flame.png');

type ResultCardProps = {
  result: SizzlingHotResult;
};

export function ResultCard({ result }: ResultCardProps) {
  const isRangeResult = result.type === 'range';

  return (
    <View style={[styles.card, isRangeResult && styles.periodCard]}>
      <View style={styles.headerRow}>
        <View style={styles.labelRow}>
          <Image source={flameIcon} style={styles.flameIcon} />
          <Text style={[styles.label, isRangeResult && styles.periodLabel]}>
            {isRangeResult ? '3-day top product' : 'Daily top product'}
          </Text>
        </View>
        <Text style={styles.date}>{result.period}</Text>
      </View>

      <Text style={styles.productName}>{result.productName}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: '#FFFFFF',
    borderColor: '#E1E4E8',
    borderLeftColor: '#F37021',
    borderLeftWidth: 5,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
  },
  date: {
    color: '#4B5563',
    flexShrink: 0,
    fontSize: 13,
    fontWeight: '600',
  },
  headerRow: {
    gap: 8,
    marginBottom: 10,
  },
  flameIcon: {
    height: 14,
    width: 14,
  },
  label: {
    color: '#7A3E00',
    flexShrink: 1,
    fontSize: 12,
    fontWeight: '700',
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  labelRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 6,
  },
  periodCard: {
    backgroundColor: '#FFF7ED',
    borderColor: '#FDBA74',
    borderLeftColor: '#007E3A',
  },
  periodLabel: {
    color: '#006B35',
  },
  productName: {
    color: '#111827',
    fontSize: 18,
    fontWeight: '700',
    lineHeight: 24,
  },
});
