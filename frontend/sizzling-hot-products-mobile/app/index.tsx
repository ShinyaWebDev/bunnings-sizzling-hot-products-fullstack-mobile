import { useCallback, useEffect, useState } from 'react';
import { router } from 'expo-router';
import {
  ActivityIndicator,
  FlatList,
  Image,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { fetchSizzlingHotResults } from '@/src/api/sizzlingHotApi';
import { ResultCard } from '@/src/components/ResultCard';
import { StateMessage } from '@/src/components/StateMessage';
import type { SizzlingHotResult } from '@/src/types/sizzlingHot';

const bunningsLogo = require('@/assets/images/bunnings-logo.jpg');
const flameIcon = require('@/assets/images/flame.png');

export default function SizzlingHotProductsScreen() {
  const [results, setResults] = useState<SizzlingHotResult[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const loadResults = useCallback(async (isRefresh = false) => {
    if (isRefresh) {
      setIsRefreshing(true);
    } else {
      setIsLoading(true);
    }

    setErrorMessage(null);

    try {
      const sizzlingHotResults = await fetchSizzlingHotResults();
      setResults(sizzlingHotResults);
    } catch {
      setErrorMessage(
        'We could not reach the backend service. Please check that the ASP.NET API is running and the mobile app is using the correct backend URL.',
      );
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  }, []);

  useEffect(() => {
    loadResults();
  }, [loadResults]);

  const handleRefresh = useCallback(() => {
    loadResults(true);
  }, [loadResults]);

  if (isLoading) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.screenPadding}>
          <Header />
          <HomeActions />
          <View style={styles.centeredContent}>
          <ActivityIndicator color="#007E3A" size="large" />
          <Text style={styles.loadingText}>Loading sizzling hot products...</Text>
          </View>
        </View>
      </SafeAreaView>
    );
  }

  if (errorMessage) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.screenPadding}>
          <Header />
          <HomeActions onRefresh={() => loadResults()} />
          <StateMessage
            title="Backend unavailable"
            message={errorMessage}
            actionLabel="Try again"
            onActionPress={() => loadResults()}
          />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safeArea}>
      <FlatList
        contentContainerStyle={styles.listContent}
        data={results}
        keyExtractor={(item) => `${item.type}-${item.period}-${item.productName}`}
        ListHeaderComponent={
          <View>
            <Header />
            <HomeActions onRefresh={handleRefresh} />
          </View>
        }
        refreshControl={
          <RefreshControl refreshing={isRefreshing} onRefresh={handleRefresh} tintColor="#007E3A" />
        }
        renderItem={({ item }) => <ResultCard result={item} />}
      />
    </SafeAreaView>
  );
}

type HomeActionsProps = {
  onRefresh?: () => void;
};

function HomeActions({ onRefresh }: HomeActionsProps) {
  return (
    <View style={styles.actionRow}>
      {onRefresh ? (
        <TouchableOpacity accessibilityRole="button" onPress={onRefresh} style={styles.refreshButton}>
          <Text style={styles.refreshButtonText}>Refresh results</Text>
        </TouchableOpacity>
      ) : null}

      <TouchableOpacity
        accessibilityRole="link"
        onPress={() => router.push('/about')}
        style={styles.aboutButton}>
        <Text style={styles.aboutButtonText}>Engineering notes</Text>
      </TouchableOpacity>
    </View>
  );
}

function Header() {
  return (
    <View style={styles.header}>
      <Image source={bunningsLogo} resizeMode="contain" style={styles.logo} />
      <View style={styles.titleRow}>
        <Image source={flameIcon} style={styles.titleIcon} />
        <Text style={styles.title}>Sizzling Hot Products</Text>
      </View>
      <Text style={styles.subtitle}>
        Calculated product leaders for 21/04/2026, 22/04/2026, 23/04/2026, and the full
        3-day period.
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  aboutButton: {
    alignItems: 'center',
    borderColor: '#D71920',
    borderRadius: 8,
    borderWidth: 1,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  aboutButtonText: {
    color: '#A81218',
    fontSize: 15,
    fontWeight: '700',
  },
  actionRow: {
    alignItems: 'center',
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
    marginBottom: 16,
  },
  centeredContent: {
    alignItems: 'center',
    flex: 1,
    justifyContent: 'center',
    padding: 24,
  },
  header: {
    marginBottom: 18,
  },
  listContent: {
    padding: 20,
    paddingBottom: 36,
  },
  loadingText: {
    color: '#4B5563',
    fontSize: 16,
    marginTop: 14,
  },
  logo: {
    height: 46,
    marginBottom: 18,
    width: 170,
  },
  refreshButton: {
    alignItems: 'center',
    alignSelf: 'flex-start',
    backgroundColor: '#007E3A',
    borderRadius: 8,
    paddingHorizontal: 16,
    paddingVertical: 11,
  },
  refreshButtonText: {
    color: '#FFFFFF',
    fontSize: 15,
    fontWeight: '700',
  },
  safeArea: {
    backgroundColor: '#F7F8FA',
    flex: 1,
  },
  screenPadding: {
    flex: 1,
    padding: 20,
  },
  subtitle: {
    color: '#4B5563',
    fontSize: 16,
    lineHeight: 23,
    marginTop: 8,
  },
  title: {
    color: '#111827',
    flex: 1,
    fontSize: 30,
    fontWeight: '800',
    letterSpacing: 0,
  },
  titleIcon: {
    height: 26,
    width: 26,
  },
  titleRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
  },
});
