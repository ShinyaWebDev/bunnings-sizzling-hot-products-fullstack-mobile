import { Link } from 'expo-router';
import { Image, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

const flameIcon = require('@/assets/images/flame.png');

const notes = [
  'The ASP.NET backend owns the business rules, aggregation, and top-product selection.',
  'The React Native Expo app is intentionally presentation-focused.',
  'Native fetch is used to avoid adding an unnecessary dependency for a simple request.',
  'TypeScript types make the API contract easier to understand and maintain.',
  'The app stays small because the take-home values correctness, clarity, and scope control.',
];

export default function AboutScreen() {
  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.titleRow}>
          <Image source={flameIcon} style={styles.flameIcon} />
          <Text style={styles.title}>Engineering Notes</Text>
        </View>

        <Text style={styles.subtitle}>
          A short summary of the implementation choices behind this mobile UI.
        </Text>

        <View style={styles.notesPanel}>
          {notes.map((note) => (
            <View key={note} style={styles.noteRow}>
              <View style={styles.bullet} />
              <Text style={styles.noteText}>{note}</Text>
            </View>
          ))}
        </View>

        <Link href="/" asChild>
          <TouchableOpacity accessibilityRole="button" style={styles.backButton}>
            <Text style={styles.backButtonText}>Back to results</Text>
          </TouchableOpacity>
        </Link>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  backButton: {
    alignItems: 'center',
    alignSelf: 'flex-start',
    backgroundColor: '#007E3A',
    borderRadius: 8,
    marginTop: 20,
    paddingHorizontal: 16,
    paddingVertical: 11,
  },
  backButtonText: {
    color: '#FFFFFF',
    fontSize: 15,
    fontWeight: '700',
  },
  bullet: {
    backgroundColor: '#D71920',
    borderRadius: 4,
    height: 8,
    marginTop: 7,
    width: 8,
  },
  content: {
    padding: 20,
    paddingBottom: 36,
  },
  flameIcon: {
    height: 24,
    width: 24,
  },
  noteRow: {
    flexDirection: 'row',
    gap: 12,
    marginBottom: 14,
  },
  noteText: {
    color: '#374151',
    flex: 1,
    fontSize: 16,
    lineHeight: 23,
  },
  notesPanel: {
    backgroundColor: '#FFFFFF',
    borderColor: '#E5E7EB',
    borderRadius: 8,
    borderWidth: 1,
    marginTop: 20,
    padding: 18,
  },
  safeArea: {
    backgroundColor: '#F7F8FA',
    flex: 1,
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
    fontSize: 28,
    fontWeight: '800',
    letterSpacing: 0,
  },
  titleRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
  },
});
